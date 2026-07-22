using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services.NotificationServices.Models;
using INotificationPublisher = Enrollify.Core.Services.NotificationServices.INotificationPublisher;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class SyncCourseCurriculumAssignmentForCurriculum
{
  public record Command (CurriculumId CurriculumId) : IRequest<Result>;

  public class Handler : IRequestHandler<Command, Result>
  {
    private readonly IReadRepository<Curriculum> _curriculumReadRepository;
    private readonly IReadRepository<AcademicYear> _academicYearReadRepository;
    private readonly IReadRepository<Course> _courseReadRepository;
    private readonly IReadRepository<CourseCurriculumAssignment> _courseCurriculumAssignmentReadRepository;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ICourseCurriculumAssignmentRepository _repository;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<Curriculum> curriculumReadRepository,
      IReadRepository<AcademicYear> academicYearReadRepository,
      IReadRepository<Course> courseReadRepository,
      IReadRepository<CourseCurriculumAssignment> courseCurriculumAssignmentReadRepository,
      INotificationPublisher notificationPublisher,
      ICourseCurriculumAssignmentRepository repository,
      ILogger<Handler> logger)
    {
      _curriculumReadRepository = curriculumReadRepository;
      _academicYearReadRepository = academicYearReadRepository;
      _courseReadRepository = courseReadRepository;
      _courseCurriculumAssignmentReadRepository = courseCurriculumAssignmentReadRepository;
      _notificationPublisher = notificationPublisher;
      _repository = repository;
      _logger = logger;
    }

    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
      var curriculum = await _curriculumReadRepository.GetByIdAsync(request.CurriculumId, cancellationToken);
      if (curriculum is null)
      {
        // TODO: Raise a notification here
        _logger.LogWarning("Curriculum with ID {CurriculumId} not found.", request.CurriculumId);
        return Result.NotFound("Curriculum not found.");
      }

      AcademicYear? academicYear = await _academicYearReadRepository.FirstOrDefaultAsync(
        new GetAcademicYearByStartDateYearSpec(curriculum.EffectiveYear), cancellationToken);

      if (academicYear is null)
      {
        // TODO: Raise a notification here
        _logger.LogWarning(
          "No academic year found for curriculum effective year {EffectiveYear} (CurriculumId: {CurriculumId}). Skipping course-curriculum assignment sync.",
          curriculum.EffectiveYear, curriculum.Id);
        return Result.NotFound("Academic year not found.");
      }

      var course = await _courseReadRepository.GetByIdAsync(curriculum.CourseId, cancellationToken);
      if (course is null)
      {
        // TODO: Raise a notification here
        _logger.LogWarning("Course with ID {CourseId} not found.", curriculum.CourseId);
        return Result.NotFound("Course not found.");
      }

      var existingAssignment = await _courseCurriculumAssignmentReadRepository.FirstOrDefaultAsync(
        new GetCourseCurriculumAssignmentByCourseAndAcademicYearSpec(course.Id, academicYear.Id), cancellationToken);

      if (existingAssignment != null && existingAssignment.IsLocked)
      {
        await _notificationPublisher.WarningTargetRoleNotification(new NotificationForTargetRoleCreation(
          "SyncCourseCurriculumAssignmentsForAcademicYear",
          "Sync Course Curriculum Assignments",
          $"Course {course.Name} already has an active curriculum assigned (CurriculumId: {existingAssignment.CurriculumId.Value}), and currently used by an existing class section(s). To replace the curriculum assigned to this course, please remove the existing class section(s) first.",
          NotificationCategoryEnum.Academic,
          [RolesEnum.SystemAdmin]
        ));
        return Result.Invalid(
          new ValidationError($"Course {course.Name} already has an active curriculum assigned (CurriculumId: {existingAssignment.CurriculumId.Value}), and currently used by an existing class section(s). To replace the curriculum assigned to this course, please remove the existing class section(s) first."));
      }

      if (existingAssignment != null)
      {
        _logger.LogInformation(
          "Updated course-curriculum assignment, From CurriculumId {PreviousCurriculumId} To {CurriculumId}",
          existingAssignment.CurriculumId.Value, curriculum.Id.Value);
        existingAssignment.UpdateCurriculum(curriculum.Id);
        Result updateResult = await _repository.BulkUpdate([existingAssignment], cancellationToken);
        return updateResult;
      }else
      {
        var newAssignment = new CourseCurriculumAssignment(course.Id, academicYear.Id, curriculum.Id);
        _logger.LogInformation(
          "Created course-curriculum assignment, CurriculumId {CurriculumId}, Course: {CourseId}",
          curriculum.Id.Value, course.Id.Value);
        Result createResult = await _repository.BulkCreate([newAssignment], cancellationToken);
        return createResult;
      }
    }
  }
}
