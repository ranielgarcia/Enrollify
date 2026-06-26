namespace Enrollify.Application.Features.Colleges.DTOs;

public class CollegeDto : BaseDto
{
    public CollegeId Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dean { get; set; } = string.Empty;

    public static CollegeDto FromEntity (College entity)
    {
        return new CollegeDto
        {
            Id = entity.Id,
            Code = entity.Code.Value,
            Name = entity.Name,
            Description = entity.Description,
            Dean = entity.Dean,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(entity.CreatedByUser),
            UpdatedBy = BaseUserDto.FromUser(entity.UpdatedByUser),
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt,
            IsActive = entity.IsActive
        };
    }
}
