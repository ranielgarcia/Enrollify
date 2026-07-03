using Enrollify.Application.Features.Courses.Events;

namespace Enrollify.Application.Features.Courses.Commands;

public static class CreateCourse
{
    public sealed record Command(CourseCode code, string name, int durationYears, string description, CollegeId collegeId)
        : IRequest<Result<CourseId>>;

    public sealed class Handler : IRequestHandler<Command, Result<CourseId>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IReadRepository<College> _collegeReadRepository;
        private readonly IMediator _mediator;

        public Handler(ICourseRepository courseRepository,
            IReadRepository<College> collegeReadRepository,
            IMediator mediator)
        {
            _courseRepository = courseRepository;
            _collegeReadRepository = collegeReadRepository;
            _mediator = mediator;
        }

        public async Task<Result<CourseId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = await _collegeReadRepository.GetByIdAsync(command.collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an id of {command.collegeId} not found");
            }

            var course = new Course(command.code, command.name, command.durationYears, command.description, command.collegeId);
            var result = await _courseRepository.Create(course, cancellationToken);

            if (result.IsSuccess)
            {
                await _mediator.Publish(new CourseCreatedEvent(result.Value), cancellationToken);
            }

            return result;
        }
    }
}
