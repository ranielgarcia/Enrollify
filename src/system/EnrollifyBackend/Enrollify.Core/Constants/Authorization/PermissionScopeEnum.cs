using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants.Authorization;

public sealed class PermissionScopeEnum : SmartEnum<PermissionScopeEnum>
{
    public static readonly PermissionScopeEnum None = new PermissionScopeEnum("None", 0);
    public static readonly PermissionScopeEnum Users = new PermissionScopeEnum("Users", 1);
    public static readonly PermissionScopeEnum Roles = new PermissionScopeEnum("Roles", 2);
    public static readonly PermissionScopeEnum Colleges = new PermissionScopeEnum("Colleges", 3);
    public static readonly PermissionScopeEnum Rooms = new PermissionScopeEnum("Rooms", 3);
    public static readonly PermissionScopeEnum RoomTypes = new PermissionScopeEnum("RoomTypes", 4);

    private PermissionScopeEnum(string name, int value) : base(name, value) { }
}
