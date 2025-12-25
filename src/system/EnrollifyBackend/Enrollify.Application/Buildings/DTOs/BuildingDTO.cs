using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.Buildings.DTOs;

public class BuildingDTO : BaseDTO
{
    public BuildingId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public CollegeSummaryDTO? College { get; set; }

    public static BuildingDTO FromEntity (Building entity)
    {
        return new BuildingDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Address = entity.Address,
            College = entity.College != null ? CollegeSummaryDTO.FromEntity(entity.College) : null,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(entity.CreatedByUser),
            UpdatedBy = BaseUserDTO.FromUser(entity.UpdatedByUser),
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt,
            IsActive = entity.IsActive
        };
    }
}
