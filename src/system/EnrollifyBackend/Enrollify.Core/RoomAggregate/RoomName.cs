using Vogen;

namespace Enrollify.Core.RoomAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct RoomName
{
    public const int MaxLength = 50;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Room name cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Room name cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
