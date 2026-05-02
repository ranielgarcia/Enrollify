using Vogen;

namespace Enrollify.Core.Aggregates.AcademicYearAggregate;

[ValueObject<int>]
public readonly partial struct Year
{
    public const int MinimumYear = 2020;
    private static Validation Validate(int year)
    {
        if (year < MinimumYear)
            return Validation.Invalid($"Year cannot be earlier than {MinimumYear}");

        return Validation.Ok;
    }
}
