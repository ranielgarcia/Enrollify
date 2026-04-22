using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.Courses.DTOs;

public class CourseDto : BaseDto
{
    public CourseId Id { get; set; }
    public CourseCode Code { get; private set; }
    public string Name { get; private set; } = null!;
    public int DurationYears { get; private set; }
    public string Description { get; private set; } = null!;

    public CollegeSummaryDto? College { get; set; }
   
    public static CourseDto FromEntity (Course entity)
    {
        return new CourseDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            DurationYears = entity.DurationYears,
            Description =  entity.Description,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(entity.CreatedByUser),
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = BaseUserDto.FromUser(entity.UpdatedByUser),
            IsActive = entity.IsActive,
            College = entity.College != null ? CollegeSummaryDto.FromEntity(entity.College) : null,
        };
    }
}
