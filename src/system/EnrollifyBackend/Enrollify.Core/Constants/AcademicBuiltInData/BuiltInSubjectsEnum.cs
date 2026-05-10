using Ardalis.SmartEnum;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Core.Constants.AcademicBuiltInData;

public sealed class BuiltInSubjectsEnum : SmartEnum<BuiltInSubjectsEnum>
{
    public SubjectCode Code { get; }
    public decimal? Units { get; set; }
    public string Description { get; }

    public static readonly BuiltInSubjectsEnum ElectivePlaceholder =
        new BuiltInSubjectsEnum("Elective Placeholder - Do not delete", "ELEC-GEN", 1, "Placeholder subject used to reserve a curriculum slot for elective course selection.", 1);

    private BuiltInSubjectsEnum(string title, string code, decimal? units, string description, int id) : base(title, id) 
    {
        Code = SubjectCode.From(code);
        Units = units;
        Description = description;
    }
}
