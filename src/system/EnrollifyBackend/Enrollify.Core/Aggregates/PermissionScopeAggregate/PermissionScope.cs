using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.PermissionScopeAggregate;

public class PermissionScope : EntityBase<PermissionScope, PermissionScopeId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private PermissionScope() { } // EF Core constructor
    public PermissionScope(string name)
    {
        Name = name;
    }
    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public string Name { get; private set; } = null!;
}
