using Enrollify.Application.Features.Departments.DTOs;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Application.SharedDTOs;

public class DepartmentSummaryDto
{
    public DepartmentId Id { get; set; }
    public DepartmentCode Code { get; set; }
    public string Name { get; set; } = null!;

    public static DepartmentSummaryDto FromEntity(Department department)
    {
        return new DepartmentSummaryDto
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
        };
    }
}
