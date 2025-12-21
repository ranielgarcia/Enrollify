using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Colleges.DTOs;

public class CollegeDTO : BaseDTO
{
    public CollegeId Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dean { get; set; } = string.Empty;
}
