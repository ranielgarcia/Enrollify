using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class StudentStatusEnum : SmartEnum<StudentStatusEnum>
{
    public string Code { get; }
    public string Description { get; }

    public static readonly StudentStatusEnum Active = new StudentStatusEnum("Active", "ACTIVE", "Currently enrolled student", 1);
    public static readonly StudentStatusEnum Inactive = new StudentStatusEnum("Inactive", "INACTIVE", "Not currently enrolled but not withdrawn", 2);
    public static readonly StudentStatusEnum LOA = new StudentStatusEnum("Leave of Absence", "LOA", "Temporarily not attending", 3);
    public static readonly StudentStatusEnum Graduated = new StudentStatusEnum("Graduated", "GRADUATED", "Completed degree requirements", 4);
    public static readonly StudentStatusEnum Withdrawn = new StudentStatusEnum("Withdrawn", "WITHDRAWN", "Permanently left the institution", 5);
    public static readonly StudentStatusEnum Suspended = new StudentStatusEnum("Suspended", "SUSPENDED", "Temporarily barred from enrollment", 6);
    public static readonly StudentStatusEnum Expelled = new StudentStatusEnum("Expelled", "EXPELLED", "Permanently barred from institution", 7);

    private StudentStatusEnum(string name, string code, string description, int value) : base(name, value) 
    {
        Code = code;
        Description = description;
    }

}
