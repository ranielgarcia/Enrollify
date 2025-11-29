using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core;

public class AuditInfo
{
    public DateTimeOffset CreatedAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public UserId? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public UserId? DeletedBy { get; private set; }
    public bool IsActive { get; private set; }

    public AuditInfo SetCreatedBy(UserId userId)
    {
        CreatedBy = userId;
        CreatedAt = DateTimeOffset.UtcNow;
        return this;
    }

    public AuditInfo SetUpdatedBy(UserId userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return this;
    }

    public AuditInfo Deactivate(UserId userId)
    {
        IsActive = false;
        DeletedBy = userId;
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
