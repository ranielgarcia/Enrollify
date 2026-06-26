using Enrollify.Application.Features.Rooms.Models;
using Enrollify.Application.SharedDTOs;

namespace Enrollify.Application.Features.Rooms.DTOs;

public class RoomDto : BaseDto
{
    public RoomId Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RoomTypeSummaryDto? RoomType { get; set; }
    public BuildingSummaryDto? Building { get; set; }

    public static RoomDto FromProjection (RoomProjection room)
    {
        return new RoomDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(room.CreatedBy),
            UpdatedAt = room.UpdatedAt,
            UpdatedBy = BaseUserDto.FromUser(room.UpdatedBy),
            IsActive = room.IsActive,
            RoomType = room.RoomType != null ? RoomTypeSummaryDto.FromProject(room.RoomType) : null,
            Building = room.Building != null ? BuildingSummaryDto.FromEntity(room.Building) : null,
        };
    }
}
