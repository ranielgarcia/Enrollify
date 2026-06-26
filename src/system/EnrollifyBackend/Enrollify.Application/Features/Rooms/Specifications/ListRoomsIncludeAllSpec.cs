using Enrollify.Application.Features.Rooms.Models;

namespace Enrollify.Application.Features.Rooms.Specifications;

public class ListRoomsIncludeAllSpec : Specification<Room, RoomProjection>
{
    public ListRoomsIncludeAllSpec() =>
        Query
        .Select(r => new RoomProjection
        {
            Id = r.Id,
            RoomNumber = r.RoomNumber,
            Capacity = r.Capacity,
            CreatedAt = r.CreatedAt,
            CreatedBy = r.CreatedByUser,
            UpdatedAt = r.UpdatedAt,
            UpdatedBy = r.UpdatedByUser,
            IsActive = r.IsActive,
            RoomType = r.RoomType == null ? null : new RoomTypeProjection
            {
                Id = r.RoomType.Id,
                Name = r.RoomType.Name
            },
            Building = r.Building == null ? null : new BuildingProjection
            {
                Id = r.Building.Id,
                Name = r.Building.Name
            }
        });
}
