using Ardalis.Result;
using Enrollify.Application.Features.Courses.Events;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Courses.Commands;

public static class CreateCourse
{
    public sealed record Command(CourseCode code, string name, int durationYears, string description, CollegeId collegeId) 
        : ICommand<Result<CourseId>>;

    public sealed class Handler : ICommandHandler<Command, Result<CourseId>>
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

        public async ValueTask<Result<CourseId>> Handle(Command command, CancellationToken cancellationToken)
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
