using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Colleges.DTOs;

public class CollegeDTO : BaseDTO
{
    public CollegeId Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dean { get; set; } = string.Empty;

    public static CollegeDTO FromEntity (College entity)
    {
        return new CollegeDTO
        {
            Id = entity.Id,
            Code = entity.Code.Value,
            Name = entity.Name,
            Description = entity.Description,
            Dean = entity.Dean,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(entity.CreatedByUser),
            UpdatedBy = BaseUserDTO.FromUser(entity.UpdatedByUser),
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt,
            IsActive = entity.IsActive
        };
    }
}
