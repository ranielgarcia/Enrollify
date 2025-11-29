using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.UserAggregate;

public class UserRoleAssignment : EntityBase<UserRoleAssignment, UserRoleAssignmentId>, IAuditable<AuditInfo>
{
    private UserRoleAssignment() { } // EF Core constructor

    public UserRoleAssignment(RoleId roleId, DateTimeOffset? expiresAt = null)
    {
        RoleId = Guard.Against.Null(roleId);
        AssignedAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
    }

    public UserId UserId { get; set; }
    public RoleId RoleId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow;

    internal UserRoleAssignment UpdateExpiration(DateTimeOffset? expiresAt)
    {
        ExpiresAt = expiresAt;
        return this;
    }
}
