namespace Enrollify.WebAPI.Authorization;

public class PolicyName
{
    public const string HasAnyValidRoleAndPermission = "HasAnyValidRoleAndPermission";

    // Roles
    public const string HasViewRolesPermission = "HasViewRolesPermission";

    // Room Types
    public const string HasCreateRoomTypePermission = "HasCreateRoomTypePermission";
}
