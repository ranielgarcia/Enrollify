using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

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
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<Curriculum> curriculumReadRepository,
      IReadRepository<AcademicYear> academicYearReadRepository,
      IReadRepository<Course> courseReadRepository,
      IReadRepository<CourseCurriculumAssignment> courseCurriculumAssignmentReadRepository,
      ILogger<Handler> logger)
    {
      _curriculumReadRepository = curriculumReadRepository;
      _academicYearReadRepository = academicYearReadRepository;
      _courseReadRepository = courseReadRepository;
      _courseCurriculumAssignmentReadRepository = courseCurriculumAssignmentReadRepository;
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


      // TODO: Implement the rest of the sync logic
      return Result.Success();
    }
  }
}
