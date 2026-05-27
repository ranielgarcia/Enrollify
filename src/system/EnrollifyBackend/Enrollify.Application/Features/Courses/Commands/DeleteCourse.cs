using Ardalis.Result;
using Enrollify.Core.Aggregates.CourseAggregate;
using MediatR;

namespace Enrollify.Application.Features.Courses.Commands;

public static class DeleteCourse
{
    public sealed record Command(CourseId id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICourseRepository _courseRepository;
        public Handler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            return await _courseRepository.Delete(command.id, cancellationToken);
        }
    }
}
