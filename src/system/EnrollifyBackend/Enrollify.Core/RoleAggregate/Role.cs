using Enrollify.SharedKernel;

namespace Enrollify.Core.RoleAggregate;

public class Role : EntityBase<Role, RoleId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private Role() { }// EF Core constructor

    public Role(RoleName name, RoleDescription description)
    {
        Name = name;
        Description = description;
    }

    public AuditInfo AuditInfo { get; set; } = new AuditInfo();

    public RoleName Name { get; private set; }

    public RoleDescription Description { get; private set; }

    public Role UpdateName (RoleName newName)
    {
        if (Name == newName) return this;
        Name = newName;

        return this;
    }

    public Role UpdateDescription (RoleDescription newDescription)
    {
        if (Description == newDescription) return this;
        Description = newDescription;

        return this;
    }
}
