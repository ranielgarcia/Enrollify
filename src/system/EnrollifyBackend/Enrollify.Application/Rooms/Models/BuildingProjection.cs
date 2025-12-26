using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.Rooms.Models;

public class BuildingProjection
{
    public BuildingId Id { get; set; }
    public string Name { get; set; } = default!;
}
