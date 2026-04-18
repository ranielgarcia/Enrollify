using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Application.Features.Departments.DTOs;

public class DepartmentDTO : BaseDTO
{
    public DepartmentId Id { get; set; }
    public DepartmentCode Code { get; set; }
    public string Name { get; set; } = null!;
    public string Chairperson { get; set; } = null!;
    public string Description { get; set; } = null!;
    public CollegeSummaryDTO College { get; set; } = null!;

    public static DepartmentDTO FromEntity(Department department)
    {
        return new DepartmentDTO
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
            Chairperson = department.Chairperson,
            Description = department.Description,
            College = CollegeSummaryDTO.FromEntity(department.College!),
            CreatedAt = department.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(department.CreatedByUser),
            UpdatedAt = department.UpdatedAt,
            UpdatedBy = department.UpdatedByUser != null ? BaseUserDTO.FromUser(department.UpdatedByUser) : null,
            IsActive = department.IsActive,
        };
    }
}
