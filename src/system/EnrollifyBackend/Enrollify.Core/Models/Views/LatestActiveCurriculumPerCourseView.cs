using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Models.Views;

public class LatestActiveCurriculumPerCourseView
{
    public CurriculumId Id { get; set; }

    public CourseId CourseId { get; set; }

    public int EffectiveYear { get; set; }

    public string Version { get; set; } = null!;

    public CurriculumStatusEnum StatusId { get; set; } = null!;
    public string? Description { get; set; }

    public DateTimeOffset? ApprovedDate { get; set; }
}
