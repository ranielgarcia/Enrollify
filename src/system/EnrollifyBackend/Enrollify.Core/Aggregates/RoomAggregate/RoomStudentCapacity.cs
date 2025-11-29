using Ardalis.GuardClauses;
using Vogen;

namespace Enrollify.Core.Aggregates.RoomAggregate;

[ValueObject<int>]
public partial struct RoomStudentCapacity
{
    public const int MinCapacity = 1;
    public const int MaxCapacity = 500;

    private static Validation Validate(int value)
    {
        Guard.Against.NegativeOrZero(value);

        if (value < MinCapacity)
            return Validation.Invalid($"Room student capacity cannot be lower than {MinCapacity}");

        if (value > MaxCapacity)
            return Validation.Invalid($"Room student capacity cannot exceed {MaxCapacity}");

        return Validation.Ok;
    }
}
