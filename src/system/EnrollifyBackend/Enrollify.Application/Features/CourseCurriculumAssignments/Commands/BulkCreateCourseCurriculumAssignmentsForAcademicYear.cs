using Ardalis.Result;
using Enrollify.Application.Features.Curriculums;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class BulkCreateCourseCurriculumAssignmentsForAcademicYear
{
    public record Command(AcademicYearId AcademicYearId) : ICommand<Result>;
    public class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICourseCurriculumAssignmentRepository _repository;
        private readonly IReadRepository<AcademicYear> _academicYearReadRepository;
        private readonly IReadRepository<Course> _courseReadRepository;
        private readonly ICurriculumRepository _curriculumRepository;

        public Handler(ICourseCurriculumAssignmentRepository repository,
            IReadRepository<AcademicYear> academicYearReadRepository,
            IReadRepository<Course> courseReadRepository,
            ICurriculumRepository curriculumRepository)
        {
            _repository = repository;
            _academicYearReadRepository = academicYearReadRepository;
            _courseReadRepository = courseReadRepository;
            _curriculumRepository = curriculumRepository;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var academicYear = await _academicYearReadRepository.GetByIdAsync(command.AcademicYearId, cancellationToken);
            if (academicYear == null)
            {
                return Result.Invalid(new ValidationError("Academic year not found."));
            }

            var courses = await _courseReadRepository.ListAsync(cancellationToken);
            var courseLatestActiveCurriculum = await _curriculumRepository.GetAllLatestActiveCurriculumPerCourse(cancellationToken);

            var courseCurriculumByCourseId = courseLatestActiveCurriculum.ToDictionary(c => c.CourseId, c => c);

            var courseCurriculumAssignmentsToCreate = new List<CourseCurriculumAssignment>();
            foreach (var course in courses)
            {
                var curriculum = courseCurriculumByCourseId.GetValueOrDefault(course.Id);

                if (curriculum == null)
                {
                    return Result.Invalid(new ValidationError($"Unable to find active curriculum for course {course.Name}"));
                }

                if (curriculum != null)
                {
                    courseCurriculumAssignmentsToCreate.Add(
                        new CourseCurriculumAssignment(course.Id, command.AcademicYearId, curriculum.Id));
                }
            }

            return await _repository.BulkCreate(courseCurriculumAssignmentsToCreate, cancellationToken);
        }
    }
}
