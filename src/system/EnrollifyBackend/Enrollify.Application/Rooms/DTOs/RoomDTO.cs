using Enrollify.Application.Rooms.Models;
using Enrollify.Core.Aggregates.RoomAggregate;

namespace Enrollify.Application.Rooms.DTOs;

public class RoomDTO : BaseDTO
{
    public RoomId Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RoomTypeSummaryDTO? RoomType { get; set; }
    public BuildingSummaryDTO? Building { get; set; }
    public CollegeSummaryDTO? College { get; set; }

    public static RoomDTO FromProjection (RoomProjection room)
    {
        return new RoomDTO
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(room.CreatedBy),
            UpdatedAt = room.UpdatedAt,
            UpdatedBy = BaseUserDTO.FromUser(room.UpdatedBy),
            IsActive = room.IsActive,
            RoomType = room.RoomType != null ? RoomTypeSummaryDTO.FromEntity(room.RoomType) : null,
            Building = room.Building != null ? BuildingSummaryDTO.FromEntity(room.Building) : null,
            College = room.College != null ? CollegeSummaryDTO.FromEntity(room.College) : null
        };
    }
}
