namespace Enrollify.Core;

public class AuditInfo
{
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public bool IsActive { get; set; }

    public AuditInfo SetCreatedBy(int userId)
    {
        CreatedBy = userId;
        CreatedAt = DateTime.UtcNow;
        return this;
    }

    public AuditInfo SetUpdatedBy(int userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public AuditInfo Deactivate(int userId)
    {
        IsActive = false;
        DeletedBy = userId;
        DeletedAt = DateTime.UtcNow;
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
