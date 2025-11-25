using Vogen;

namespace Enrollify.Core.UserAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct UserFirstName
{
    public const int MaxLength = 50;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Firstname cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Firstname cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
