using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Application.Features.Departments.DTOs;

public class DepartmentDto : BaseDto
{
    public DepartmentId Id { get; set; }
    public DepartmentCode Code { get; set; }
    public string Name { get; set; } = null!;
    public string Chairperson { get; set; } = null!;
    public string Description { get; set; } = null!;
    public CollegeSummaryDto College { get; set; } = null!;

    public static DepartmentDto FromEntity(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
            Chairperson = department.Chairperson,
            Description = department.Description,
            College = CollegeSummaryDto.FromEntity(department.College!),
            CreatedAt = department.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(department.CreatedByUser),
            UpdatedAt = department.UpdatedAt,
            UpdatedBy = department.UpdatedByUser != null ? BaseUserDto.FromUser(department.UpdatedByUser) : null,
            IsActive = department.IsActive,
        };
    }
}
