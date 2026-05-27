using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class StudentTypeEnum : SmartEnum<StudentTypeEnum>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public int DisplayOrder { get; set; }

    public static readonly StudentTypeEnum Regular = new StudentTypeEnum("Regular", 1, "REGULAR", "Standard first-year admission; follows cohort curriculum", 1);
    public static readonly StudentTypeEnum Irregular = new StudentTypeEnum("Irregular", 2, "IRREGULAR", "Not following standard year-term progression; enrolls per subject", 2);
    public static readonly StudentTypeEnum Transferee = new StudentTypeEnum("Transferee", 3, "TRANSFEREE", "Transferred from another institution; subject crediting required", 3);
    public static readonly StudentTypeEnum Shifter = new StudentTypeEnum("Shifter",4, "SHIFTER", "Changed degree program within same institution", 4);
    public static readonly StudentTypeEnum Returnee = new StudentTypeEnum("Returnee", 5, "RETURNEE", "Returned after Leave of Absence; may require curriculum bridging", 5);
    public static readonly StudentTypeEnum CrossEnrollee = new StudentTypeEnum("Cross-Enrollee", 6, "CROSS_ENROLLEE", "Enrolled from another institution for specific subjects", 6);

    public StudentTypeEnum(string name, int value, string code, string description, int displayOrder) : base(name, value)
    {
        Code = code;
        Description = description;
        DisplayOrder = displayOrder;
    }
}
