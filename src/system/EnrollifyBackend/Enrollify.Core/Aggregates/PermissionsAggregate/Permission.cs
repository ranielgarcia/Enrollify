using Enrollify.Core.Aggregates.PermissionsAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.PermissionsAggregate;

public class Permission : EntityBase<Permission, PermissionId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private Permission() { } // EF Core constructor
    public Permission(PermissionName name, PermissionResource resource, PermissionAction action)
    {
        Name = name;
        Resource = resource;
        Action = action;
    }
    public PermissionName Name { get; private set; }
    public PermissionResource Resource { get; private set; }
    public PermissionAction Action { get; private set; }

    public PermissionDescription Description { get; private set; }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();



    public Permission UpdateName(PermissionName newName)
    {
        if (Name == newName) return this;
        Name = newName;
        return this;
    }

    public Permission UpdateResource(PermissionResource newResource)
    {
        if (Resource == newResource) return this;
        Resource = newResource;
        return this;
    }

    public Permission UpdateAction(PermissionAction newAction)
    {
        if (Action == newAction) return this;
        Action = newAction;
        return this;
    }

    public Permission UpdateDescription (PermissionDescription newDescription)
    {
        if (Description == newDescription) return this;
        Description = newDescription;
        return this;
    }
}
