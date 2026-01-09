using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class CurriculumStatusEnum : SmartEnum<CurriculumStatusEnum>
{
    public static readonly CurriculumStatusEnum Draft = new CurriculumStatusEnum("Draft", 1);
    public static readonly CurriculumStatusEnum Active = new CurriculumStatusEnum("Active", 2);
    public static readonly CurriculumStatusEnum PhaseOut = new CurriculumStatusEnum("PhaseOut", 3);
    public static readonly CurriculumStatusEnum Archived = new CurriculumStatusEnum("Archived", 4);

    private CurriculumStatusEnum(string name, int value) : base(name, value) { }
}
