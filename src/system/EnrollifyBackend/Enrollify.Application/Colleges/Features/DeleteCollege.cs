using Ardalis.Result;
using Enrollify.Application.Rooms;
using Enrollify.Application.Rooms.Features;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Mediator;

namespace Enrollify.Application.Colleges.Features;

public static class DeleteCollege
{
    public sealed record Command(CollegeId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICollegeRepository _collegeRepository;
        private readonly IMediator _mediator;

        public Handler(ICollegeRepository collegeRepository, IMediator mediator)
        {
            _collegeRepository = collegeRepository;
            _mediator = mediator;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var count = await _mediator.Send(new CountRoomsByCollegeQuery { CollegeId = command.id}, cancellationToken);
            if (count.Value > 0)
            {
                return Result.Invalid(new ValidationError($"This college cannot be deleted because it has {count.Value} room(s) associated with it. \n Please reassign or remove these rooms from this college before deleting."));
            }
            // TODO: Implement this:
            //var courses = await _courseRepository.GetAllByCollege(command.id, cancellationToken);
            //if (courses.Count > 0)
            //{
            //    return Result.Invalid(new ValidationError($"This college cannot be deleted because it has {courses.Count} course(s) associated with it. \n Please reassign or remove these courses from this college before deleting."));
            //}
            return await _collegeRepository.Delete(command.id, cancellationToken);
        }
    }
}
