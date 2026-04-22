using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.Features.Buildings.DTOs;

public class BuildingDto : BaseDto
{
    public BuildingId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public CollegeSummaryDto? College { get; set; }

    public static BuildingDto FromEntity (Building entity)
    {
        return new BuildingDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Address = entity.Address,
            College = entity.College != null ? CollegeSummaryDto.FromEntity(entity.College) : null,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(entity.CreatedByUser),
            UpdatedBy = BaseUserDto.FromUser(entity.UpdatedByUser),
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt,
            IsActive = entity.IsActive
        };
    }
}
