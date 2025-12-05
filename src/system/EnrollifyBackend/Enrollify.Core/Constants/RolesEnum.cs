using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class RolesEnum : SmartEnum<RolesEnum>
{
    public static readonly RolesEnum SystemAdmin = new RolesEnum(nameof(SystemAdmin), 1);
    public static readonly RolesEnum Admin = new RolesEnum(nameof(Admin), 2);
    public static readonly RolesEnum Registrar = new RolesEnum(nameof(Registrar), 3);
    public static readonly RolesEnum FinanceOfficer = new RolesEnum(nameof(FinanceOfficer), 4);
    public static readonly RolesEnum AdmissionOfficer = new RolesEnum(nameof(AdmissionOfficer), 5);
    public static readonly RolesEnum DepartmentHead = new RolesEnum(nameof(DepartmentHead), 6);
    public static readonly RolesEnum AcademicAdvisor = new RolesEnum(nameof(AcademicAdvisor), 7);
    public static readonly RolesEnum ScholarshipCoordinator = new RolesEnum(nameof(ScholarshipCoordinator), 8);
    public static readonly RolesEnum Teacher = new RolesEnum(nameof(Teacher), 9);
    public static readonly RolesEnum ProgramCoordinator = new RolesEnum(nameof(ProgramCoordinator), 10);
    public static readonly RolesEnum Student = new RolesEnum(nameof(Student), 11);
    public static readonly RolesEnum ParentOrGuardian = new RolesEnum(nameof(ParentOrGuardian), 12);

    private RolesEnum(string name, int value) : base(name, value)
    {
    }
}
