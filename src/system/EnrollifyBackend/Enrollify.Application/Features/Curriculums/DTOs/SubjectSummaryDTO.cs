using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Features.Curriculums.DTOs;

public class SubjectSummaryDTO
{
    public SubjectId Id { get; set; }
    public SubjectCode Code { get; set; }
    public string Title { get; set; } = null!;
    public decimal Units { get; set; }

    public static SubjectSummaryDTO FromEntity(Subject subject)
    {
        return new SubjectSummaryDTO
        {
            Id = subject.Id,
            Code = subject.Code,
            Title = subject.Title,
            Units = subject.Units
        };
    }
}
