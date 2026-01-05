using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class CurriculaStatusEnum : SmartEnum<CurriculaStatusEnum>
{
    public static readonly CurriculaStatusEnum Draft = new CurriculaStatusEnum("Draft", 1);
    public static readonly CurriculaStatusEnum Active = new CurriculaStatusEnum("Active", 2);
    public static readonly CurriculaStatusEnum PhaseOut = new CurriculaStatusEnum("PhaseOut", 3);
    public static readonly CurriculaStatusEnum Archived = new CurriculaStatusEnum("Archived", 4);

    private CurriculaStatusEnum(string name, int value) : base(name, value) { }
}
