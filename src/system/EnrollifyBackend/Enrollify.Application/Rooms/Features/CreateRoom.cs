using Ardalis.Result;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Mediator;

namespace Enrollify.Application.Rooms.Features;

public static class CreateRoom
{
    public sealed record Command(string roomNumber, int capacity, RoomTypeId roomTypeId, BuildingId buildingId, CollegeId collegeId)
        : ICommand<Result<RoomId>>;

    public sealed class Handler : ICommandHandler<Command, Result<RoomId>>
    {
        public Handler() { }

        public ValueTask<Result<RoomId>> Handle(Command command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
