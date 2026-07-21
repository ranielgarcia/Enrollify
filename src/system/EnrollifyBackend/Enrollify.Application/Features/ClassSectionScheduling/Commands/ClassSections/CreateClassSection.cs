using Enrollify.Application.Features.ClassSectionScheduling.Extensions;
using Enrollify.Application.Features.CourseCurriculumAssignments;
using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate.Models;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services.NotificationServices.Models;
using Enrollify.Core.ValueObjects;
using INotificationPublisher = Enrollify.Core.Services.NotificationServices.INotificationPublisher;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;

public static class CreateClassSection
{
  public sealed record Command(
    YearLevel YearLevel,
    CourseId CourseId,
    AcademicTermId AcademicTermId,
    TeacherId AdviserId,
    int StudentCapacity) : IRequest<Result<ClassSectionId>>;

  public sealed class Handler : IRequestHandler<Command, Result<ClassSectionId>>
  {
    private readonly IReadRepository<AcademicYear> _academicYearReadRepository;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IReadRepository<CourseCurriculumAssignment> _courseCurriculumAssignmentReadRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IClassSectionSubjectOfferingRepository _classSectionSubjectOfferingRepository;
    private readonly ICourseCurriculumAssignmentRepository _courseCurriculumAssignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventBus _eventBus;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<AcademicYear> academicYearReadRepository,
      IReadRepository<ClassSection> classSectionReadRepository,
      IReadRepository<CourseCurriculumAssignment> courseCurriculumAssignmentReadRepository,
      IClassSectionRepository classSectionRepository,
      IClassSectionSubjectOfferingRepository classSectionSubjectOfferingRepository,
      ICourseCurriculumAssignmentRepository courseCurriculumAssignmentRepository,
      IUnitOfWork unitOfWork,
      IDomainEventBus eventBus,
      INotificationPublisher notificationPublisher,
      ILogger<Handler> logger)
    {
      _academicYearReadRepository = academicYearReadRepository;
      _classSectionReadRepository = classSectionReadRepository;
      _courseCurriculumAssignmentReadRepository = courseCurriculumAssignmentReadRepository;
      _classSectionRepository = classSectionRepository;
      _classSectionSubjectOfferingRepository = classSectionSubjectOfferingRepository;
      _courseCurriculumAssignmentRepository = courseCurriculumAssignmentRepository;
      _unitOfWork = unitOfWork;
      _eventBus = eventBus;
      _notificationPublisher = notificationPublisher;
      _logger = logger;
    }


    private Year GetCohortEntryYear(AcademicYear classSectionAcademicYear, YearLevel yearLevel)
    {
      return Year.From(classSectionAcademicYear.StartDate.Value.Year - (yearLevel.Value - 1));
    }


    private async Task<Result<(AcademicYear cohortAcademicYear, CourseCurriculumAssignment
        cohortCourseCurriculumAssignment)>>
      GetCohortCourseCurriculumAssignment
      (CourseId courseId, Year cohortEntryYear, CancellationToken ct)
    {
      AcademicYear? academicYear = await _academicYearReadRepository
        .FirstOrDefaultAsync(new GetAcademicYearByStartDateYearSpec(cohortEntryYear), CancellationToken.None);

      // Limitation: Requires all historical curriculum data for the cohort's entry academic year.
      // If the academic year or curriculum assignments are missing, bulk initialization fails.
      // Admins must pre-populate all required historical data before creating sections for higher year levels.
      if (academicYear == null) return Result.Error("Academic year for the given cohort entry year not found.");

      CourseCurriculumAssignment? courseCurriculumAssignment = await _courseCurriculumAssignmentReadRepository
        .FirstOrDefaultAsync(
          new GetCourseCurriculumAssignmentByCourseAndAcademicYearSpec(courseId, academicYear.Id), ct);

      if (courseCurriculumAssignment == null)
        return Result.Error("Course-Curriculum assignment for the given cohort not found.");

      return Result.Success((academicYear, courseCurriculumAssignment));
    }


    public async Task<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
    {
      // Get validated entities (we know they exist because of validation)
      AcademicYear? academicYear = await _academicYearReadRepository
        .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.AcademicTermId),
          cancellationToken);
      AcademicTerm academicTerm = academicYear!.AcademicTerms.First(at => at.Id == command.AcademicTermId);


      Year intendedCohortEntryYear = GetCohortEntryYear(academicYear, command.YearLevel);

      Result<(AcademicYear cohortAcademicYear, CourseCurriculumAssignment cohortCourseCurriculumAssignment)>
        getCohortCourseCurriculumAssignmentResult =
          await GetCohortCourseCurriculumAssignment(command.CourseId, intendedCohortEntryYear,
            cancellationToken);

      if (!getCohortCourseCurriculumAssignmentResult.IsSuccess)
        return Result.Error(string.Join("; ", getCohortCourseCurriculumAssignmentResult.Errors));

      (AcademicYear cohortAcademicYear, CourseCurriculumAssignment courseCurriculumAssignment) =
        getCohortCourseCurriculumAssignmentResult.Value;

      Course? course = courseCurriculumAssignment.Course;
      Curriculum? curriculum = courseCurriculumAssignment.Curriculum;

      // Generate section code
      SectionCode sectionCode = await GetSectionCode(command, cancellationToken);

      // Get subjects that should be offered for this class section
      IEnumerable<CurriculumSubject>? curriculumSubjects =
        curriculum?.GetSubjectsByYearAndTerm(command.YearLevel, academicTerm.TermNumber);
      if (curriculumSubjects == null || !curriculumSubjects.Any())
      {
        _logger.LogWarning(
          "No curriculum subjects found for course with an id of {CourseId}, year level of {YearLevel}, and term number of {TermNumber}.",
          command.CourseId, command.YearLevel, academicTerm.TermNumber);
        return Result.Error("No curriculum subjects found for the specified course, year level, and term.");
      }

      // Begin transaction to ensure all database operations succeed or fail together
      await using ITransactionScope transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

      try
      {
        // Create the class section
        var newClassSection = new ClassSection(new ClassSectionForCreation
        {
          Name = course!.Code.Value,
          IntendedYearLevel = command.YearLevel,
          CourseId = command.CourseId,
          CurriculumId = curriculum!.Id,
          AcademicTermId = command.AcademicTermId,
          AdviserId = command.AdviserId,
          SectionCode = sectionCode,
          CohortAcademicYearId = cohortAcademicYear.Id,
          InitializeStatus = ClassSectionStatusEnum.PendingValidation
        });
        Result<ClassSectionId> createResult =
          await _classSectionRepository.Create(newClassSection, cancellationToken);
        if (!createResult.IsSuccess)
        {
          _logger.LogError("Failed to create class section: {Errors}",
            string.Join(", ", createResult.Errors));
          return Result.Error("Unable to create the class section.");
        }

        ClassSectionId classSectionId = createResult.Value;

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
            await _classSectionSubjectOfferingRepository.Create(newSubjectOffering,
              cancellationToken);

          if (!offeringResult.IsSuccess)
          {
            _logger.LogError(
              "Failed to create subject offering for ClassSection {ClassSectionId}, Subject {SubjectId}: {Errors}",
              classSectionId,
              curriculumSubject.SubjectId,
              string.Join(", ", offeringResult.Errors));
            return Result.Error("Unable to create one or more subject offerings.");
          }
        }

        courseCurriculumAssignment.Lock($"This curriculum is used as reference for class sections in {academicTerm.TermName}, year level {command.YearLevel} for course {course.Name}.");
        await _courseCurriculumAssignmentRepository.BulkUpdate([courseCurriculumAssignment], cancellationToken);

        // TODO: Use the correct Target Role
        await _notificationPublisher.SuccessTargetRoleNotification(new NotificationForTargetRoleCreation(
          "CreateClassSection",
          "Create Class Section",
          $"Successfully created {newClassSection.FullName} for {course.Name} for term {academicTerm.TermName}, year level {command.YearLevel}.",
          NotificationCategoryEnum.Academic,
          [RolesEnum.SystemAdmin]
        ));

        // Publish after commit so the event handler reads fully-committed data.
        // ClassSectionCreatedEvent is routed to a dedicated BufferedInMemory local queue (see
        // ConfigureWolverine in InfrastructureServiceExtensions), so the cascading validation-issue
        // recomputation chain runs asynchronously in the background rather than blocking this call.
        await _eventBus.PublishAsync(new ClassSectionCreatedEvent(classSectionId));

        // Use the unit of work's outbox-aware commit instead of transaction.CommitAsync() directly:
        // it saves pending changes, commits the ambient transaction, and flushes the published
        // ClassSectionCreatedEvent to the outbox message store all together. Calling
        // transaction.CommitAsync() alone would commit the DB writes but never flush the outbox,
        // silently dropping the published event. This call returns once the flush hands the event
        // off to its (async) local queue - it does not wait for downstream handlers to finish.
        await _unitOfWork.SaveChangesAndFlushMessagesThenCommitAsync(cancellationToken);

        _logger.LogInformation(
          "Successfully created class section {ClassSectionId} with {SubjectCount} subject offerings",
          classSectionId,
          curriculumSubjects.Count());

        return Result.Success(createResult.Value);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating class section and subject offerings");
        // Transaction will rollback automatically on dispose
        return Result.Error("An unexpected error occurred while creating the class section.");
      }
    }

    private async Task<SectionCode> GetSectionCode(Command command, CancellationToken ct)
    {
      List<ClassSection>? existingClassSections = await _classSectionReadRepository.ListAsync(
        new GetExistingClassSectionsByCourseYearLevelAndTerm(command.YearLevel, command.CourseId,
          command.AcademicTermId), ct);
      SectionCode? lastExistingClassSectionCode = existingClassSections?.OrderByDescending(cs => cs.SectionCode)
        .FirstOrDefault()?.SectionCode;

      return lastExistingClassSectionCode.GetNextSectionCode();
    }
  }
}
