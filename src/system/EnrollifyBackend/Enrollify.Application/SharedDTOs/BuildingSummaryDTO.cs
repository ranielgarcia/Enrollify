using Enrollify.Application.Features.Rooms.Models;

namespace Enrollify.Application.SharedDTOs;

public class BuildingSummaryDto
{
    public BuildingId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static BuildingSummaryDto FromEntity(BuildingProjection entity)
    {
        return new BuildingSummaryDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
