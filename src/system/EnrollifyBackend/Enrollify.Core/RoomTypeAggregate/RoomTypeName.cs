using Vogen;

namespace Enrollify.Core.RoomTypeAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct RoomTypeName
{
    public const int MaxLength = 50;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Room type name cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Room type name cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
