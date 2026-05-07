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

    // Subjects
    public const string HasViewSubjectsPermission = "HasViewSubjectsPermission";
    public const string HasCreateSubjectPermission = "HasCreateSubjectPermission";
    public const string HasUpdateSubjectPermission = "HasUpdateSubjectPermission";
    public const string HasDeleteSubjectPermission = "HasDeleteSubjectPermission";


    // Curriculums
    public const string HasViewCurriculumsPermission = "HasViewCurriculumsPermission";
    public const string HasCreateCurriculumPermission = "HasCreateCurriculumPermission";
    public const string HasUpdateCurriculumPermission = "HasUpdateCurriculumPermission";
    public const string HasDeleteCurriculumPermission = "HasDeleteCurriculumPermission";


    // Subject Equivalence Groups
    public const string HasCreateSubjectEquivalenceGroupPermission = "HasCreateSubjectEquivalenceGroupPermission";
    public const string HasUpdateSubjectEquivalenceGroupPermission = "HasUpdateSubjectEquivalenceGroupPermission";
    public const string HasDeleteSubjectEquivalenceGroupPermission = "HasDeleteSubjectEquivalenceGroupPermission";
    public const string HasViewSubjectEquivalenceGroupsPermission = "HasViewSubjectEquivalenceGroupsPermission";

    // Teachers

    public const string HasCreateTeacherPermission = "HasCreateTeacherPermission";
    public const string HasUpdateTeacherPermission = "HasUpdateTeacherPermission";
    public const string HasDeleteTeacherPermission = "HasDeleteTeacherPermission";
    public const string HasViewTeacherPermission = "HasViewTeacherPermission";


    // Academic Years and Terms
    public const string HasCreateAcademicYearAndTermPermission = "HasCreateAcademicYearAndTermPermission";
    public const string HasUpdateAcademicYearAndTermPermission = "HasUpdateAcademicYearAndTermPermission";
    public const string HasDeleteAcademicYearAndTermPermission = "HasDeleteAcademicYearAndTermPermission";
    public const string HasViewAcademicYearAndTermPermission = "HasViewAcademicYearAndTermPermission";


    // Class Sections
    public const string HasCreateClassSectionPermission = "HasCreateClassSectionPermission";
    public const string HasUpdateClassSectionPermission = "HasUpdateClassSectionPermission";
    public const string HasDeleteClassSectionPermission = "HasDeleteClassSectionPermission";
    public const string HasViewClassSectionPermission = "HasViewClassSectionPermission";

}
