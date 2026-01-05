using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Aggregates.CurriculaAggregate.Models;

public class CurriculaForCreation
{
    public CourseId CourseId { get; set; }
    public int EffectiveYear { get; set; }

    public string Version { get; set; } = null!;
    public CurriculaStatusEnum StatusId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset? ApprovedDate { get; set; }

}
