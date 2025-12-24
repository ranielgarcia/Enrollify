using Ardalis.Specification;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Rooms.Specifications;

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
            },
            College = r.College == null ? null : new CollegeProjection
            {
                Id = r.College.Id,
                Name = r.College.Name
            }
        });
}

public class RoomProjection
{
    public RoomId Id { get; set; }
    public string RoomNumber { get; set; } = default!;
    public int Capacity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public User? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public User? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public RoomTypeProjection? RoomType { get; set; }
    public BuildingProjection? Building { get; set; }
    public CollegeProjection? College { get; set; }
}

public class RoomTypeProjection
{
    public RoomTypeId Id { get; set; }
    public string Name { get; set; } = default!;
}

public class BuildingProjection
{
    public BuildingId Id { get; set; }
    public string Name { get; set; } = default!;
}

public class CollegeProjection
{
    public CollegeId Id { get; set; }
    public string Name { get; set; } = default!;
}
