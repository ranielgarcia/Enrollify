using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.Buildings.DTOs;

public class BuildingDTO : BaseDTO
{
    public BuildingId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
