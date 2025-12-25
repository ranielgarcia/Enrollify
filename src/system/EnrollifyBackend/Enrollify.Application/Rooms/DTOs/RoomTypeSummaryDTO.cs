using Enrollify.Application.Rooms.Models;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.Rooms.DTOs;

public class RoomTypeSummaryDTO
{
    public RoomTypeId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static RoomTypeSummaryDTO FromEntity(RoomTypeProjection entity)
    {
        return new RoomTypeSummaryDTO
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}