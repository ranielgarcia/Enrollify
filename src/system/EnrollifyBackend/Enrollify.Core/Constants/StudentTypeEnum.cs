using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class StudentTypeEnum : SmartEnum<StudentTypeEnum>
{
    public string Code { get; set; }
    public string Description { get; set; }

    public static readonly StudentTypeEnum Regular = new StudentTypeEnum("Regular", 1, "REGULAR", "Standard first-year admission; follows cohort curriculum");
    public static readonly StudentTypeEnum Irregular = new StudentTypeEnum("Irregular", 2, "IRREGULAR", "Not following standard year-term progression; enrolls per subject");
    public static readonly StudentTypeEnum Transferee = new StudentTypeEnum("Transferee", 3, "TRANSFEREE", "Transferred from another institution; subject crediting required");
    public static readonly StudentTypeEnum Shifter = new StudentTypeEnum("Shifter",4, "SHIFTER", "Changed degree program within same institution");
    public static readonly StudentTypeEnum Returnee = new StudentTypeEnum("Returnee", 5, "RETURNEE", "Returned after Leave of Absence; may require curriculum bridging");
    public static readonly StudentTypeEnum CrossEnrollee = new StudentTypeEnum("Cross-Enrollee", 6, "CROSS_ENROLLEE", "Enrolled from another institution for specific subjects");

    public StudentTypeEnum(string name, int value, string code, string description) : base(name, value)
    {
        Code = code;
        Description = description;
    }
}
