using Ardalis.Result;
using Enrollify.Core.Aggregates.CourseAggregate;
using Mediator;

namespace Enrollify.Application.Courses.Specifications;

public static class DeleteCourse
{
    public sealed record Command(CourseId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICourseRepository _courseRepository;
        public Handler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            return await _courseRepository.Delete(command.id, cancellationToken);
        }
    }
}
