using Enrollify.Application.Rooms.Models;
using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.SharedDTOs;

public class BuildingSummaryDTO
{
    public BuildingId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static BuildingSummaryDTO FromEntity(BuildingProjection entity)
    {
        return new BuildingSummaryDTO
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
