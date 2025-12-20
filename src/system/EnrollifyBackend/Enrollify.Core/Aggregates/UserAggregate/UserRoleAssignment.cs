using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.RoleAggregate;

namespace Enrollify.Core.Aggregates.UserAggregate;

public class UserRoleAssignment
{
    private UserRoleAssignment() { } // EF Core constructor

    public UserRoleAssignment(RoleId roleId, UserId assignedBy, DateTimeOffset? expiresAt = null)
    {
        RoleId = Guard.Against.Null(roleId);
        AssignedAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
    }

    public UserId UserId { get; set; }
    public RoleId RoleId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }


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


    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow;

    internal UserRoleAssignment UpdateExpiration(DateTimeOffset? expiresAt)
    {
        ExpiresAt = expiresAt;
        return this;
    }
}
