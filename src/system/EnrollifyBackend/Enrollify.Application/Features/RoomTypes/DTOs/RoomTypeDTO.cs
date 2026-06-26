namespace Enrollify.Application.Features.RoomTypes.DTOs;

public class RoomTypeDto : BaseDto
{
    public RoomTypeId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public static RoomTypeDto FromEntity (RoomType entity)
    {
        return new RoomTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(entity.CreatedByUser),
            UpdatedBy = BaseUserDto.FromUser(entity.UpdatedByUser),
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt,
            IsActive = entity.IsActive
        };
    }
}
