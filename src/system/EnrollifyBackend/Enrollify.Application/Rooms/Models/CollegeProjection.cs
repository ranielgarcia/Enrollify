using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Rooms.Models;

public class CollegeProjection
{
    public CollegeId Id { get; set; }
    public string Name { get; set; } = default!;
}
