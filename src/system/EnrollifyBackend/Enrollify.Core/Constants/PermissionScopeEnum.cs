using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class PermissionScopeEnum : SmartEnum<PermissionScopeEnum>
{
    public static readonly PermissionScopeEnum Users = new PermissionScopeEnum("Users", 1);

    private PermissionScopeEnum(string name, int value) : base(name, value)
    {
        
    }
}
