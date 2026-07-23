using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Application.Features.Notifications;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services;
using Enrollify.Core.Services.NotificationServices.Models;
using INotificationPublisher = Enrollify.Core.Services.NotificationServices.INotificationPublisher;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class SyncCourseCurriculumAssignmentsForAcademicYear
{
  public record Command(AcademicYearId AcademicYearId) : IRequest<Result>;

  public class Handler : IRequestHandler<Command, Result>
  {
    private readonly ICourseCurriculumAssignmentRepository _repository;
    private readonly IReadRepository<CourseCurriculumAssignment> _readRepository;
    private readonly IReadRepository<Course> _courseReadRepository;
    private readonly IReadRepository<AcademicYear> _academicYearReadRepository;
    private readonly IApplicableCurriculumQueryService _applicableCurriculumQueryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<Handler> _logger;

    public Handler(
      ICourseCurriculumAssignmentRepository repository,
      IReadRepository<CourseCurriculumAssignment> readRepository,
      IReadRepository<Course> courseReadRepository,
      IReadRepository<AcademicYear> academicYearReadRepository,
      IApplicableCurriculumQueryService applicableCurriculumQueryService,
      IUnitOfWork unitOfWork,
      INotificationPublisher notificationPublisher,
      ILogger<Handler> logger)
    {
      _repository = repository;
      _readRepository = readRepository;
      _courseReadRepository = courseReadRepository;
      _academicYearReadRepository = academicYearReadRepository;
      _applicableCurriculumQueryService = applicableCurriculumQueryService;
      _unitOfWork = unitOfWork;
      _notificationPublisher = notificationPublisher;
      _logger = logger;
    }

    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
      AcademicYear? academicYear = await _academicYearReadRepository.GetByIdAsync(request.AcademicYearId, cancellationToken);
      if (academicYear is null)
      {
        _logger.LogError("Unable to find the academic year with an id of {AcademicYearId}", request.AcademicYearId);
        return Result.NotFound("Academic year not found.");
      }

      List<Course> courses = await _courseReadRepository.ListAsync(cancellationToken);
      var courseIds = courses.Select(c => c.Id).ToList();
      List<CourseCurriculumAssignment> existingAssignments = await _readRepository.ListAsync(
        new GetAllCourseCurriculumAssignmentsForCoursesByAcademicYearIdSpec(courseIds, academicYear.Id),
        cancellationToken);

      // Get the latest active curriculum for each course that is applicable to the academic year's start date/year
      IReadOnlyList<Curriculum> applicableCurriculums =
        await _applicableCurriculumQueryService.GetApplicableCurriculumsForAcademicYearStartDateAsync(
          academicYear.StartDate, cancellationToken);

      var applicableCurriculumsByCourseId = applicableCurriculums.ToDictionary(c => c.CourseId, c => c);
      var existingAssignmentByCourseId = existingAssignments.ToDictionary(c => c.CourseId, c => c);

      var courseCurriculumAssignmentsToCreate = new List<CourseCurriculumAssignment>();
      var courseCurriculumAssignmentsToUpdate = new List<CourseCurriculumAssignment>();
      foreach (Course course in courses)
      {
        CourseCurriculumAssignment? existing = existingAssignmentByCourseId.GetValueOrDefault(course.Id);

        if (existing != null && existing.IsLocked)
        {
          await _notificationPublisher.WarningTargetRoleNotification(new NotificationForTargetRoleCreation(
            NotificationTypeConstants.SyncCourseCurriculumAssignmentsForAcademicYear,
            "Sync Course Curriculum Assignments",
            $"Course {course.Name} already has an active curriculum assigned (CurriculumId: {existing.CurriculumId}), and currently used by an existing class section(s). To replace the curriculum assigned to this course, please remove the existing class section(s) first.",
            NotificationCategoryEnum.Academic,
            [RolesEnum.SystemAdmin]
          ));
          continue;
        }

        if (existing?.Curriculum?.StatusId == CurriculumStatusEnum.Active)
        {
          await _notificationPublisher.WarningTargetRoleNotification(new NotificationForTargetRoleCreation(
            NotificationTypeConstants.SyncCourseCurriculumAssignmentsForAcademicYear,
            "Sync Course Curriculum Assignments",
            $"Course {course.Name} already has an active curriculum assigned (CurriculumId: {existing.CurriculumId}), skipping assignment.",
            NotificationCategoryEnum.Academic,
            [RolesEnum.SystemAdmin]
          ));
          continue;
        }

        Curriculum? curriculum = applicableCurriculumsByCourseId.GetValueOrDefault(course.Id);

        if (curriculum == null)
        {
          await _notificationPublisher.WarningTargetRoleNotification(new NotificationForTargetRoleCreation(
            NotificationTypeConstants.SyncCourseCurriculumAssignmentsForAcademicYear,
            "Sync Course Curriculum Assignments",
            $"Unable to find an active curriculum for course {course.Name} and academic year {academicYear.AcademicYearTitle}, skipping assignment.",
            NotificationCategoryEnum.Academic,
            [RolesEnum.SystemAdmin]
          ));
          _logger.LogWarning("Unable to find an active curriculum for course {CourseName} and academic year {AcademicYear}", course.Name, academicYear.AcademicYearTitle);
          // return Result.Error($"Unable to find an active curriculum for course {course.Name}");
          continue;
        }

        if (existing != null && existing.CurriculumId != curriculum.Id)
        {
          _logger.LogInformation(
            "Updated course-curriculum assignment, From CurriculumId {PreviousCurriculumId} To {CurriculumId}",
            existing.CurriculumId, curriculum.Id);
          existing.UpdateCurriculum(curriculum.Id);
          courseCurriculumAssignmentsToUpdate.Add(existing);
        }

        if (existing == null)
        {
          var newAssignment = new CourseCurriculumAssignment(course.Id, academicYear.Id, curriculum.Id);
          courseCurriculumAssignmentsToCreate.Add(newAssignment);
        }
      }

      // Begin transaction to ensure all database operations succeed or fail together
      await using ITransactionScope transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

      try
      {
        if (courseCurriculumAssignmentsToCreate.Any())
        {
          Result bulkCreateResult = await _repository.BulkCreate(courseCurriculumAssignmentsToCreate, cancellationToken);
          if (!bulkCreateResult.IsSuccess)
          {
            _logger.LogError("Bulk Create - Failed to sync course-curriculum assignments. Reasons: {@ErrorMessages}",
              string.Join(", ", bulkCreateResult.Errors));
            await transaction.RollbackAsync(cancellationToken);
            return Result.Error("Failed to sync course-curriculum assignments.");
          }
        }

        if (courseCurriculumAssignmentsToUpdate.Any())
        {
          Result bulkUpdateResult = await _repository.BulkUpdate(courseCurriculumAssignmentsToUpdate, cancellationToken);
          if (!bulkUpdateResult.IsSuccess)
          {
            _logger.LogError("Bulk Update - Failed to sync course-curriculum assignments. Reasons: {@ErrorMessages}",
              string.Join(", ", bulkUpdateResult.Errors));
            await transaction.RollbackAsync(cancellationToken);
            return Result.Error("Failed to sync course-curriculum assignments.");
          }
        }

        if (courseCurriculumAssignmentsToUpdate.Any() || courseCurriculumAssignmentsToCreate.Any())
        {
          await _unitOfWork.SaveChangesAndFlushMessagesThenCommitAsync(cancellationToken);
        }

        return Result.Success();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to sync course-curriculum assignments.");
        return Result.Error("Failed to sync course-curriculum assignments.");
      }
    }
  }
}
