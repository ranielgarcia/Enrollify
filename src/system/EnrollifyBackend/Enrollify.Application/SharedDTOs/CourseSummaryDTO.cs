using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.SharedDTOs;

public class CourseSummaryDTO
{
    public CourseId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static CourseSummaryDTO FromEntity(Course course)
    {
        return new CourseSummaryDTO
        {
            Id = course.Id,
            Name = course.Name
        };
    }
}
