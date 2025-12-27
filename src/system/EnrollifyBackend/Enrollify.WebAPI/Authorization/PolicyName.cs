namespace Enrollify.WebAPI.Authorization;

public class PolicyName
{
    // Shared Policies
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
    public const string HasCreateRoomPermission = "HasCreateRoomPermission";
    public const string HasUpdateRoomPermission = "HasUpdateRoomPermission";
    public const string HasDeleteRoomPermission = "HasDeleteRoomPermission";

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

    // Departments
    public const string HasViewDepartmentPermission = "HasViewDepartmentPermission";
    public const string HasCreateDepartmentPermission = "HasCreateDepartmentPermission";
    public const string HasUpdateDepartmentPermission = "HasUpdateDepartmentPermission";
    public const string HasDeleteDepartmentPermission = "HasDeleteDepartmentPermission";


    // Courses
    public const string HasViewCoursesPermission = "HasViewCoursesPermission";
    public const string HasCreateCoursePermission = "HasCreateCoursePermission";
    public const string HasUpdateCoursePermission = "HasUpdateCoursePermission";
    public const string HasDeleteCoursePermission = "HasDeleteCoursePermission";
}
