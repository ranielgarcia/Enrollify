namespace Enrollify.Application.SharedDTOs;

public class CourseSummaryDto
{
    public CourseId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static CourseSummaryDto FromEntity(Course course)
    {
        return new CourseSummaryDto
        {
            Id = course.Id,
            Name = course.Name
        };
    }
}
