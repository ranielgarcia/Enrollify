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
    public static readonly PermissionScopeEnum RoomTypes = new PermissionScopeEnum("RoomTypes", 5);
    public static readonly PermissionScopeEnum Buildings = new PermissionScopeEnum("Buildings", 6);
    public static readonly PermissionScopeEnum Departments = new PermissionScopeEnum("Departments", 7);
    public static readonly PermissionScopeEnum Courses = new PermissionScopeEnum("Courses", 8);
    public static readonly PermissionScopeEnum Subjects = new PermissionScopeEnum("Subjects", 9);
    public static readonly PermissionScopeEnum Curriculums = new PermissionScopeEnum("Curriculums", 10);
    public static readonly PermissionScopeEnum SubjectEquivalenceGroups = new PermissionScopeEnum("SubjectEquivalenceGroups", 11);
    public static readonly PermissionScopeEnum Teachers = new PermissionScopeEnum("Teachers", 12);
    public static readonly PermissionScopeEnum AcademicYearsAndTerms = new PermissionScopeEnum("AcademicYearsAndTerms", 13);
    public static readonly PermissionScopeEnum ClassSections = new PermissionScopeEnum("ClassSections", 14);

    private PermissionScopeEnum(string name, int value) : base(name, value) { }
}
