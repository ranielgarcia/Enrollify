using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core;

public class AuditInfo
{
    public DateTimeOffset CreatedAt { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public int? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public int? DeletedBy { get; private set; }
    public bool IsActive { get; private set; }

    public AuditInfo SetCreatedBy(UserId userId)
    {
        CreatedBy = userId.Value;
        CreatedAt = DateTimeOffset.UtcNow;
        return this;
    }

    public AuditInfo SetUpdatedBy(UserId userId)
    {
        UpdatedBy = userId.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
        return this;
    }

    public AuditInfo Deactivate(UserId userId)
    {
        IsActive = false;
        DeletedBy = userId.Value;
        DeletedAt = DateTimeOffset.UtcNow;
        return this;
    }

    public AuditInfo Activate()
    {
        IsActive = true;
        DeletedBy = null;
        DeletedAt = null;
        return this;
    }
}
