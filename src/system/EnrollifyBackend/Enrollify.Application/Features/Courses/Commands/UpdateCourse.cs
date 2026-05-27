using Ardalis.Result;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Courses.Commands;

public static class UpdateCourse
{
    public sealed record Command(CourseId id, CourseCode code, string name, int durationYears, string description, CollegeId collegeId)
        : IRequest<Result<CourseId>>;

    public sealed class Handler : IRequestHandler<Command, Result<CourseId>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IReadRepository<Course> _courseReadRepository;
        private readonly IReadRepository<College> _collegeReadRepository;

        public Handler(
            ICourseRepository courseRepository,
            IReadRepository<Course> courseReadRepository,
            IReadRepository<College> collegeReadRepository)
        {
            _courseRepository = courseRepository;
            _courseReadRepository = courseReadRepository;
            _collegeReadRepository = collegeReadRepository;
        }

        public async Task<Result<CourseId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = await _collegeReadRepository.GetByIdAsync(command.collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an id of {command.collegeId} not found");
            }

            var existing = await _courseReadRepository.GetByIdAsync(command.id, cancellationToken);
            if (existing == null)
            {
                return Result.NotFound($"Course with an id of {command.id} not found");
            }

            existing.UpdateCode(command.code);
            existing.UpdateName(command.name);
            existing.UpdateDurationYears(command.durationYears);
            existing.UpdateDescription(command.description);
            existing.UpdateCollegeId(command.collegeId);

            var updateResult = await _courseRepository.Update(existing, cancellationToken);
            return updateResult;
        }
    }
}
