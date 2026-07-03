namespace Enrollify.Application.SharedDTOs;

public class CollegeSummaryDto
{
    public CollegeId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static CollegeSummaryDto FromEntity(College entity)
    {
        return new CollegeSummaryDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

}
