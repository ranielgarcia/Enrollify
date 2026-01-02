using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Subjects.DTOs;

public class CourseSummaryDTO
{
    public CourseId Id { get; set; }
    public CourseCode Code { get; set; }
    public string Name { get; set; } = null!;

    public static CourseSummaryDTO FromEntity(Course course)
    {
        return new CourseSummaryDTO
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
        };
    }
}
