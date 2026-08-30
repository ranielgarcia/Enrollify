using Enrollify.Application.Features.ClassSectionScheduling.Extensions;
using Enrollify.Application.Features.CourseCurriculumAssignments;
using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Application.Features.Notifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate.Models;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services.NotificationServices.Models;
using Enrollify.Core.ValueObjects;
using INotificationPublisher = Enrollify.Core.Services.NotificationServices.INotificationPublisher;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;

public static class BulkInitializeClassSectionsForAcademicYear
{
  public sealed record TargetCourse(CourseId CourseId, int NumberOfSections);

  public sealed record Command(AcademicTermId AcademicTermId, YearLevel YearLevel, List<TargetCourse> TargetCourses)
    : IRequest<Result>;

  public sealed class Handler : IRequestHandler<Command, Result>
  {
    private readonly IReadRepository<AcademicYear> _academicYearRepository;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IReadRepository<CourseCurriculumAssignment> _courseCurriculumAssignmentsReadRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IClassSectionSubjectOfferingRepository _classSectionSubjectOfferingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationEventDispatcher _eventDispatcher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ICourseCurriculumAssignmentRepository _courseCurriculumAssignmentRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<AcademicYear> academicYearRepository,
      IReadRepository<ClassSection> classSectionReadRepository,
      IReadRepository<CourseCurriculumAssignment> courseCurriculumAssignmentsReadRepository,
      IClassSectionRepository classSectionRepository,
      IClassSectionSubjectOfferingRepository classSectionSubjectOfferingRepository,
      IUnitOfWork unitOfWork,
      IApplicationEventDispatcher eventDispatcher,
      INotificationPublisher notificationPublisher,
      ICourseCurriculumAssignmentRepository courseCurriculumAssignmentRepository,
      ICurrentUserAccessor currentUserAccessor,
      ILogger<Handler> logger)
    {
      _academicYearRepository = academicYearRepository;
      _classSectionReadRepository = classSectionReadRepository;
      _courseCurriculumAssignmentsReadRepository = courseCurriculumAssignmentsReadRepository;
      _classSectionRepository = classSectionRepository;
      _classSectionSubjectOfferingRepository = classSectionSubjectOfferingRepository;
      _unitOfWork = unitOfWork;
      _eventDispatcher = eventDispatcher;
      _notificationPublisher = notificationPublisher;
      _courseCurriculumAssignmentRepository = courseCurriculumAssignmentRepository;
      _currentUserAccessor = currentUserAccessor;
      _logger = logger;
    }


    public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
    {
      // Capture the initiating user now, while HTTP context is still alive.
      // This is propagated through the Wolverine event chain so background handlers
      // know which user to send real-time invalidations to.
      UserId? triggeredBy = _currentUserAccessor.GetCurrentUser()?.Id;

      var targetCourseIds = command.TargetCourses.Select(p => p.CourseId).ToList();

      // Get academic year and term (validated by validator)
      AcademicYear? academicYear = await _academicYearRepository
        .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.AcademicTermId), cancellationToken);
      AcademicTerm academicTerm = academicYear!.AcademicTerms.First(at => at.Id == command.AcademicTermId);

      Year intendedCohortEntryYear = GetCohortEntryYear(academicYear, command.YearLevel);

      Result<(AcademicYear cohortAcademicYear, List<CourseCurriculumAssignment> cohortCourseCurriculumAssignments)>
        getCohortCourseCurriculumResult =
          await GetCohortCourseCurriculumAssignments(targetCourseIds, intendedCohortEntryYear, cancellationToken);
      if (!getCohortCourseCurriculumResult.IsSuccess)
        return Result.Error(string.Join("; ", getCohortCourseCurriculumResult.Errors));

      (AcademicYear cohortAcademicYear, List<CourseCurriculumAssignment> cohortCourseCurriculumAssignments) =
        getCohortCourseCurriculumResult.Value;

      var courseCurriculumAssignmentsByCourseId =
        cohortCourseCurriculumAssignments.ToDictionary(x => x.CourseId, x => x);

      List<ClassSection> existingSections = await _classSectionReadRepository.ListAsync(
        new GetExistingClassSectionsByCourseYearLevelAndTerm(
          new List<YearLevel> { command.YearLevel },
          targetCourseIds,
          new List<AcademicTermId> { command.AcademicTermId }),
        cancellationToken);
      var existingSectionsByCourseId =
        existingSections.GroupBy(s => s.CourseId).ToDictionary(g => g.Key, g => g.ToList());

      // H5: Pre-validate that every course has subjects for the given year level and term before opening the transaction.
      // Fails fast without any DB writes if any course's curriculum has no subjects for this year/term.
      foreach (TargetCourse targetCourse in command.TargetCourses)
      {
        CourseCurriculumAssignment assignment = courseCurriculumAssignmentsByCourseId[targetCourse.CourseId];
        if (!assignment.Curriculum!.GetSubjectsByYearAndTerm(command.YearLevel, academicTerm.TermNumber).Any())
        {
          // TODO: Raise a notification here to inform the user in the client app
          _logger.LogWarning(
            "No curriculum subjects found for course {CourseId}, year level {YearLevel}, term {TermNumber}",
            targetCourse.CourseId, command.YearLevel, academicTerm.TermNumber);
          return Result.Error(
            $"No curriculum subjects found for course '{assignment.Course!.Code.Value}', year level {command.YearLevel}, term {academicTerm.TermNumber}.");
        }
      }

      await using ITransactionScope transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

      int totalSectionsCreated = 0;
      var createdSectionIds = new List<ClassSectionId>();
      try
      {
        int totalSectionsCreatedForCurrentCourse = 0;
        foreach (TargetCourse targetCourse in command.TargetCourses)
        {
          CourseCurriculumAssignment courseCurriculumAssignment =
            courseCurriculumAssignmentsByCourseId[targetCourse.CourseId];
          Curriculum curriculum = courseCurriculumAssignment.Curriculum!;
          Course course = courseCurriculumAssignment.Course!;

          var curriculumSubjects = curriculum
            .GetSubjectsByYearAndTerm(command.YearLevel, academicTerm.TermNumber)
            .ToList();

          // Get existing sections to determine starting section code
          List<ClassSection>? existingClassSections =
            existingSectionsByCourseId.GetValueOrDefault(targetCourse.CourseId);
          SectionCode? lastExistingClassSectionCode = existingClassSections?
            .OrderByDescending(cs => cs.SectionCode)
            .FirstOrDefault()?
            .SectionCode;

          totalSectionsCreatedForCurrentCourse = 0;
          // Create the requested number of sections
          for (int i = 0; i < targetCourse.NumberOfSections; i++)
          {
            // Generate next section code (A, B, C, etc.)
            SectionCode sectionCode = lastExistingClassSectionCode.GetNextSectionCode();
            lastExistingClassSectionCode = sectionCode;

            // Create class section with auto-generated name
            var newClassSection = new ClassSection(new ClassSectionForCreation
            {
              Name = course.Code.Value,
              IntendedYearLevel = command.YearLevel,
              CourseId = targetCourse.CourseId,
              CurriculumId = curriculum.Id,
              AcademicTermId = command.AcademicTermId,
              AdviserId = null, // No adviser assigned during bulk initialization
              SectionCode = sectionCode,
              CohortAcademicYearId = cohortAcademicYear.Id,
              InitializeStatus = ClassSectionStatusEnum.PendingValidation,
            });

            Result<ClassSectionId> createResult =
              await _classSectionRepository.Create(newClassSection, cancellationToken);
            if (!createResult.IsSuccess)
            {
              _logger.LogError("Failed to create class section: {Errors}", string.Join(", ", createResult.Errors));
              return Result.Error($"Unable to create class section for course '{course.Code.Value}'.");
            }

            ClassSectionId classSectionId = createResult.Value;

            // Create subject offerings for this section
            foreach (CurriculumSubject curriculumSubject in curriculumSubjects)
            {
              decimal snapshotUnits = curriculumSubject.SubjectUnitsOverride
                                      ?? curriculumSubject.Subject?.Units
                                      ?? throw new InvalidOperationException("Subject units not found");
              var newSubjectOffering = new ClassSectionSubjectOffering(new ClassSectionSubjectOfferingForCreation
              {
                SubjectId = curriculumSubject.SubjectId,
                ClassSectionId = classSectionId,
                CurriculumSubjectId = curriculumSubject.Id,
                DaysPerWeek = curriculumSubject.DaysPerWeek,
                HoursPerDay = curriculumSubject.HoursPerDay,
                SnapshotSubjectCode = curriculumSubject.Subject!.Code,
                SnapshotSubjectTitle = curriculumSubject.Subject!.Title,
                SnapshotUnits = snapshotUnits,
                SnapshotIsElective = curriculumSubject.IsElective,
                SnapshotElectiveGroupName = curriculumSubject.ElectiveGroupName
              });

              Result<ClassSectionSubjectOfferingId> offeringResult =
                await _classSectionSubjectOfferingRepository.Create(
                  newSubjectOffering,
                  cancellationToken);

              if (!offeringResult.IsSuccess)
              {
                _logger.LogError(
                  "Failed to create subject offering for ClassSection {ClassSectionId}, Subject {SubjectId}: {Errors}",
                  classSectionId, curriculumSubject.SubjectId, string.Join(", ", offeringResult.Errors));
                return Result.Error("Unable to create one or more subject offerings.");
              }
            }

            createdSectionIds.Add(classSectionId);
            totalSectionsCreated++;
            totalSectionsCreatedForCurrentCourse++;
            _logger.LogInformation(
              "Created class section {SectionName} (ID: {ClassSectionId}) with {SubjectCount} subject offerings",
              newClassSection.Name, classSectionId, curriculumSubjects.Count);
          }

          // Lock the Course-Curriculumn Assignment
          courseCurriculumAssignment.Lock($"This curriculum is used as reference for class sections in {academicTerm.TermName}, year level {command.YearLevel} for course {course.Name}.");

          // TODO: Use the correct Target Role
          await _notificationPublisher.SuccessTargetRoleNotification(new NotificationForTargetRoleCreation(
            NotificationTypeConstants.BulkInitializeClassSectionsForAcademicYear,
            "Bulk Initialize Class Sections",
            $"Successfully initialized {totalSectionsCreatedForCurrentCourse} class section(s) for {course.Name} for term {academicTerm.TermName}, year level {command.YearLevel}.",
            NotificationCategoryEnum.Academic,
            [RolesEnum.SystemAdmin]
          ));
        }

        Result lockResult = await _courseCurriculumAssignmentRepository.BulkUpdate(cohortCourseCurriculumAssignments, cancellationToken);
        if (!lockResult.IsSuccess)
        {
          _logger.LogError(
            "Failed to lock course-curriculum assignments. Errors: {Errors}",
            string.Join(", ", lockResult.Errors));
          await transaction.RollbackAsync(cancellationToken);
          return Result.Error("Unable to lock course-curriculum assignments.");
        }

        foreach (ClassSectionId createdSectionId in createdSectionIds)
        {
          await _eventDispatcher.DispatchDeferredAsync(new ClassSectionCreatedEvent(createdSectionId, triggeredBy), cancellationToken);
        }

        // ClassSectionCreatedEvent is routed to a dedicated BufferedInMemory local queue (see
        // ConfigureWolverine in InfrastructureServiceExtensions), so the resulting validation-issue
        // recomputation chain (OnClassSectionCreatedEventHandler -> RefreshClassSectionValidationIssuesRequestedEvent
        // -> ComputeAndGetValidationIssuesForClassSection) runs asynchronously in the background.
        // This call returns as soon as the class sections are committed and the events are handed
        // off - it does not wait for those handlers to finish.
        await _unitOfWork.SaveChangesAndFlushMessagesThenCommitAsync(cancellationToken);


        _logger.LogInformation(
          "Successfully bulk initialized {TotalSections} class sections for term {TermId}, year level {YearLevel}",
          totalSectionsCreated, command.AcademicTermId, command.YearLevel);

        return Result.Success();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during bulk class section initialization");
        // Transaction will rollback automatically on dispose
        return Result.Error("An unexpected error occurred while initializing class sections.");
      }
    }


    //> Creating class sections for BSCS in AY 2025-2026:

    //| Year Level | Entry AY     | Lookup in `CourseCurriculumAssignments`  |
    //| ---------- | ------------ | ---------------------------------------- |
    //| Year 1     | AY 2025-2026 | (BSCS, AY 2025-2026) → Curriculum 2025-A |
    //| Year 2     | AY 2024-2025 | (BSCS, AY 2024-2025) → Curriculum 2024-A |
    //| Year 3     | AY 2023-2024 | (BSCS, AY 2023-2024) → Curriculum 2023-X |
    //| Year 4     | AY 2022-2023 | (BSCS, AY 2022-2023) → Curriculum 2023-X |

    //The entry AY derivation lives in application code: find the AcademicYear with `StartDate.Year = currentAY.StartDate.Year - (YearLevel - 1)`.

    private async Task<Result<(AcademicYear cohortAcademicYear, List<CourseCurriculumAssignment>
        cohortCourseCurriculumAssignments)>>
      GetCohortCourseCurriculumAssignments
      (List<CourseId> courseIds, Year cohortEntryYear, CancellationToken ct)
    {
      AcademicYear? academicYear = await _academicYearRepository
        .FirstOrDefaultAsync(new GetAcademicYearByStartDateYearSpec(cohortEntryYear), ct);

      // Limitation: Requires all historical curriculum data for the cohort's entry academic year.
      // If the academic year or curriculum assignments are missing, bulk initialization fails.
      // Admins must pre-populate all required historical data before creating sections for higher year levels.
      if (academicYear == null) return Result.Error("Academic year for the given cohort entry year not found.");

      List<CourseCurriculumAssignment> courseCurriculumAssignments = await _courseCurriculumAssignmentsReadRepository
        .ListAsync(new GetAllCourseCurriculumAssignmentsForCoursesByAcademicYearIdSpec(courseIds, academicYear.Id), ct);

      return Result.Success((academicYear, courseCurriculumAssignments));
    }

    private Year GetCohortEntryYear(AcademicYear classSectionAcademicYear, YearLevel yearLevel)
    {
      return Year.From(classSectionAcademicYear.StartDate.Value.Year - (yearLevel.Value - 1));
    }
  }
}
