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

    // Colleges
    public const string HasCreateCollegePermission = "HasCreateCollegePermission";
    public const string HasUpdateCollegePermission = "HasUpdateCollegePermission";
    public const string HasDeleteCollegePermission = "HasDeleteCollegePermission";
    public const string HasViewCollegePermission = "HasViewCollegePermission";

    // Buildings
    public const string HasViewBuildingPermission = "HasViewBuildingPermission";
    public const string HasCreateBuildingPermission = "HasCreateBuildingPermission";
    public const string HasUpdateBuildingPermission = "HasUpdateBuildingPermission";
    public const string HasDeleteBuildingPermission = "HasDeleteBuildingPermission";
}
