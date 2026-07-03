namespace Enrollify.Application.SharedDTOs;

public class SubjectSummaryDto
{
    public SubjectId Id { get; set; }
    public SubjectCode Code { get; set; }
    public string Title { get; set; } = null!;
    public decimal Units { get; set; }

    public static SubjectSummaryDto FromEntity(Subject subject)
    {
        return new SubjectSummaryDto
        {
            Id = subject.Id,
            Code = subject.Code,
            Title = subject.Title,
            Units = subject.Units
        };
    }
}
