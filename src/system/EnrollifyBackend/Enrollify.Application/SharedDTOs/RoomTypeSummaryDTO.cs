using Enrollify.Application.Features.Rooms.Models;

namespace Enrollify.Application.SharedDTOs;

public class RoomTypeSummaryDto
{
    public RoomTypeId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static RoomTypeSummaryDto FromEntity (RoomType entity)
    {
        return new RoomTypeSummaryDto
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }

    public static RoomTypeSummaryDto FromProject(RoomTypeProjection entity)
    {
        return new RoomTypeSummaryDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
