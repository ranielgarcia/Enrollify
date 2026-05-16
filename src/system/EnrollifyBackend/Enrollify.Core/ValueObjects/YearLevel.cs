using Vogen;

namespace Enrollify.Core.ValueObjects;

[ValueObject<int>]
public readonly partial struct YearLevel
{
    public const int MaximumLevel = 6;
    public const int MinimumLevel = 1;
    private static Validation Validate(int level)
    {
        if (level < MinimumLevel)
            return Validation.Invalid($"Level cannot be less than {MinimumLevel}");

        if (level > MaximumLevel)
            return Validation.Invalid($"Level cannot be greater than {MaximumLevel}");

        return Validation.Ok;
    }
}
