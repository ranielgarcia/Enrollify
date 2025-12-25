using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Buildings.DTOs;

public class CollegeSummaryDTO
{
    public CollegeId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static CollegeSummaryDTO FromEntity(College entity)
    {
        return new CollegeSummaryDTO
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

}
