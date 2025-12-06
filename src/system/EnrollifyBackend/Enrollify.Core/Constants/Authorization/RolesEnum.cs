using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants.Authorization;

public sealed class RolesEnum : SmartEnum<RolesEnum>
{
    public string Description { get; }

    public static readonly RolesEnum SystemAdmin = 
        new RolesEnum(nameof(SystemAdmin), 1,
            """
            The Super Admin holds the highest level of access within the 
            system and has unrestricted control over all modules, 
            settings, and data. This role is responsible for configuring global 
            system parameters, managing all user accounts and permissions, overseeing security policies, 
            and performing critical administrative operations.
            """);

    public static readonly RolesEnum Admin = 
        new RolesEnum(nameof(Admin), 2,
            """
            Responsible for managing day-to-day system operations within the scope defined by 
            the Super Admin. This role oversees users, content, or configurations for their assigned tenant,
            department, or environment but does not have access to system-level settings.
            """);

    public static readonly RolesEnum Registrar =
        new RolesEnum(nameof(Registrar), 3,
            """
            Manage academic calendars and enrollment periods
            Approve/reject enrollment requests and course add/drops
            Handle course section creation and scheduling
            Process transcript requests and academic records
            Manage student status (active, LOA, graduated, etc.)
            Override system blocks and restrictions
            """);

    public static readonly RolesEnum FinanceOfficer =
        new RolesEnum(nameof(FinanceOfficer), 4,
            """
            Process tuition and fee payments
            Generate billing statements and assessment of fees
            Apply discounts, scholarships, and financial aid
            Issue official receipts
            Manage payment plans and installments
            Clear/block students based on payment status
            """);

    public static readonly RolesEnum AdmissionOfficer = 
        new RolesEnum(nameof(AdmissionOfficer), 5,
            """
            Process new student applications
            Evaluate admission requirements
            Assign student numbers
            Handle student type classification (freshman, transferee, etc.)
            Manage application status workflow
            """);

    public static readonly RolesEnum DepartmentHead = 
        new RolesEnum(nameof(DepartmentHead), 6,
            """
            Approve course offerings for their department
            Manage faculty teaching loads
            Override enrollment capacity limits
            Handle special enrollment cases (overload, prerequisites waiver)
            Review and approve class schedules
            """);

    public static readonly RolesEnum AcademicAdvisor =
        new RolesEnum(nameof(AcademicAdvisor), 7,
            """
            Review and approve student study plans
            Monitor academic progress and standing
            Advise on course selection and prerequisites
            Handle curriculum compliance checks
            """);

    public static readonly RolesEnum ScholarshipCoordinator =
        new RolesEnum(nameof(ScholarshipCoordinator), 8,
            """
            Manage scholarship programs and eligibility
            Process scholarship applications
            Apply scholarship discounts to student accounts
            Monitor scholarship retention requirements
            """);

    public static readonly RolesEnum Teacher = 
        new RolesEnum(nameof(Teacher), 9,
            """
            View class rosters and student information
            Input and manage grades
            Mark attendance
            Drop students from classes
            View teaching schedules and room assignments
            """);

    public static readonly RolesEnum ProgramCoordinator = 
        new RolesEnum(nameof(ProgramCoordinator), 10,
            """
            Manage curriculum and course offerings for specific programs
            Monitor program enrollment numbers
            Handle program-specific requirements
            """);

    public static readonly RolesEnum Student = 
        new RolesEnum(nameof(Student), 11,
            """
            Enroll in courses during enrollment period
            View assessment and payment status
            View class schedules, grades, and attendance
            Request for documents (COR, grades, etc.)
            Add/drop courses within allowed period
            """);

    public static readonly RolesEnum ParentOrGuardian = 
        new RolesEnum(nameof(ParentOrGuardian), 12,
            """
            View student grades and attendance
            View billing and payment status
            Receive notifications about academic performance
            """);

    private RolesEnum(string name, int value, string description) : base(name, value)
    {
        Description = description;
    }
}
