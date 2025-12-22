using Ardalis.Result;
using Enrollify.Application.Rooms;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Mediator;

namespace Enrollify.Application.Colleges.Features;

public static class DeleteCollege
{
    public sealed record Command(CollegeId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICollegeRepository _collegeRepository;
        private readonly IRoomRepository _roomRepository;
        public Handler(ICollegeRepository collegeRepository, IRoomRepository roomRepository)
        {
            _collegeRepository = collegeRepository;
            _roomRepository = roomRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var rooms = await _roomRepository.GetAllByCollege(command.id, cancellationToken);
            if (rooms.Count > 0)
            {
                return Result.Invalid(new ValidationError($"This college cannot be deleted because it has {rooms.Count} room(s) associated with it. \n Please reassign or remove these rooms from this college before deleting."));
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
