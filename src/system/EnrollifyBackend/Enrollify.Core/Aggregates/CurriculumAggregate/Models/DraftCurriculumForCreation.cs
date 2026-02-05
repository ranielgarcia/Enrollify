using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Core.Aggregates.CurriculumAggregate.Models;

public class DraftCurriculumForCreation
{
    public CourseId CourseId { get; set; }
    public int EffectiveYear { get; set; }

    public string Version { get; set; } = null!;
    public string? Description { get; set; }
}
