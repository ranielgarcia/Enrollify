using Enrollify.Application.Rooms.Models;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Rooms.DTOs;

public class CollegeSummaryDTO
{
    public CollegeId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public static CollegeSummaryDTO FromEntity(CollegeProjection entity)
    {
        return new CollegeSummaryDTO
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }
}
