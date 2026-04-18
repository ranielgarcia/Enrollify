using Enrollify.Application.Features.Rooms.Models;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.SharedDTOs;

public class RoomTypeSummaryDTO
{
    public RoomTypeId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static RoomTypeSummaryDTO FromEntity (RoomType entity)
    {
        return new RoomTypeSummaryDTO
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }

    public static RoomTypeSummaryDTO FromProject(RoomTypeProjection entity)
    {
        return new RoomTypeSummaryDTO
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}