using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants.Authorization;

public sealed class PermissionScopeEnum : SmartEnum<PermissionScopeEnum>
{
    // Reminder: Be mindful when changing the values of existing enums, as they are stored in the database and
    // the value is considered an identifier (Id value).
    // This enum must be kept in sync with `AuthorizationScope.ts` in the client application.
    public static readonly PermissionScopeEnum None = new PermissionScopeEnum("None", 0);
    public static readonly PermissionScopeEnum Users = new PermissionScopeEnum("Users", 1);
    public static readonly PermissionScopeEnum Roles = new PermissionScopeEnum("Roles", 2);
    public static readonly PermissionScopeEnum Colleges = new PermissionScopeEnum("Colleges", 3);
    public static readonly PermissionScopeEnum Rooms = new PermissionScopeEnum("Rooms", 4);
    public static readonly PermissionScopeEnum RoomTypes = new PermissionScopeEnum("Room Types", 5);
    public static readonly PermissionScopeEnum Buildings = new PermissionScopeEnum("Buildings", 6);

    private PermissionScopeEnum(string name, int value) : base(name, value) { }
}
