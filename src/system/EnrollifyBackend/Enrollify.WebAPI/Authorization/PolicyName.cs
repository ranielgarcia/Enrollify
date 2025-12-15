namespace Enrollify.WebAPI.Authorization;

public class PolicyName
{
    public const string HasAnyValidRoleAndPermission = "HasAnyValidRoleAndPermission";

    // Roles
    public const string HasViewRolesPermission = "HasViewRolesPermission";

    // Room Types
    public const string HasCreateRoomTypePermission = "HasCreateRoomTypePermission";
    public const string HasViewRoomTypesPermission = "HasViewRoomTypesPermission";
    public const string HasUpdateRoomTypesPermission = "HasUpdateRoomTypesPermission";
    public const string HasDeleteRoomTypesPermission = "HasDeleteRoomTypesPermission";

    // Rooms
    public const string HasViewRoomsPermission = "HasViewRoomsPermission";
}
