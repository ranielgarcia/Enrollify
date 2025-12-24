using Enrollify.Application.Buildings.DTOs;
using Enrollify.Application.Colleges.DTOs;
using Enrollify.Application.Rooms.Specifications;
using Enrollify.Application.RoomTypes.DTOs;
using Enrollify.Core.Aggregates.RoomAggregate;

namespace Enrollify.Application.Rooms.DTOs;

public class RoomDTO : BaseDTO
{
    public RoomId Id { get; set; }
    public string RoomNumber { get; set; }
    public int Capacity { get; set; }
    public RoomTypeDTO? RoomType { get; set; }
    public BuildingDTO? Building { get; set; }
    public CollegeDTO? College { get; set; }

    public static RoomDTO FromEntity (Room room)
    {
        return new RoomDTO
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            RoomType = room.RoomType != null ? RoomTypeDTO.FromEntity(room.RoomType) : null,
            Building = room.Building != null ? BuildingDTO.FromEntity(room.Building) : null,
            College = room.College != null ? CollegeDTO.FromEntity(room.College) : null
        };
    }

    public static RoomDTO FromProjection(RoomProjection projection)
    {
        return new RoomDTO
        {
            Id = projection.Id,
            RoomNumber = projection.RoomNumber,
            Capacity = projection.Capacity,
            CreatedAt = projection.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(projection.CreatedBy),
            UpdatedBy = BaseUserDTO.FromUser(projection.UpdatedBy),
            UpdatedAt = projection.UpdatedAt,
            IsActive = projection.IsActive,
            RoomType = projection.RoomType != null ? new RoomTypeDTO
            {
                Id = projection.RoomType.Id,
                Name = projection.RoomType.Name
            } : null,
            Building = projection.Building != null ? new BuildingDTO
            {
                Id = projection.Building.Id,
                Name = projection.Building.Name
            } : null,
            College = projection.College != null ? new CollegeDTO
            {
                Id = projection.College.Id,
                Name = projection.College.Name
            } : null
        };
    }
}
