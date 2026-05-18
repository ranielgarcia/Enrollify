using Vogen;

namespace Enrollify.Core.ValueObjects;

[ValueObject<int>]
public readonly partial struct Year
{
    public const int MinimumYear = 2000;
    private static Validation Validate(int year)
    {
        if (year < MinimumYear)
            return Validation.Invalid($"Year cannot be earlier than {MinimumYear}");

        return Validation.Ok;
    }
}
