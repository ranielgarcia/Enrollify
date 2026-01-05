using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CurriculaAggregate;

public class Curricula : EntityBase<Curricula, CurriculaId>, IAggregateRoot, IAuditable
{
    private Curricula(){}

    public CourseId CourseId { get; private set; }
    public int EffectiveYear { get; private set; } // Academic year when this curriculum takes effect (e.g., 2024)

    public string Version { get; private set; } // Version identifier (e.g. '2025-A', '2025-REV1')
    public CurriculaStatusEnum StatusId { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset? ApprovedDate { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public User? CreatedByUser { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public UserId? UpdatedBy { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public UserId? DeletedBy { get; private set; }
    public User? DeletedByUser { get; private set; }
    public bool IsActive { get; private set; }

}
