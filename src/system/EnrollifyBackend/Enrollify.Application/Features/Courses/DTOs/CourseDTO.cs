using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.Courses.DTOs;

public class CourseDTO : BaseDTO
{
    public CourseId Id { get; set; }
    public CourseCode Code { get; private set; }
    public string Name { get; private set; } = null!;
    public int DurationYears { get; private set; }
    public string Description { get; private set; } = null!;

    public CollegeSummaryDTO? College { get; set; }
   
    public static CourseDTO FromEntity (Course entity)
    {
        return new CourseDTO
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            DurationYears = entity.DurationYears,
            Description =  entity.Description,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(entity.CreatedByUser),
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = BaseUserDTO.FromUser(entity.UpdatedByUser),
            IsActive = entity.IsActive,
            College = entity.College != null ? CollegeSummaryDTO.FromEntity(entity.College) : null,
        };
    }
}
