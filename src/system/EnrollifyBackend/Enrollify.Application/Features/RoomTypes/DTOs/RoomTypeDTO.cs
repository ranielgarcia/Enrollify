using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.Features.RoomTypes.DTOs;

public class RoomTypeDTO : BaseDTO
{
    public RoomTypeId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public static RoomTypeDTO FromEntity (RoomType entity)
    {
        return new RoomTypeDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(entity.CreatedByUser),
            UpdatedBy = BaseUserDTO.FromUser(entity.UpdatedByUser),
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt,
            IsActive = entity.IsActive
        };
    }
}
