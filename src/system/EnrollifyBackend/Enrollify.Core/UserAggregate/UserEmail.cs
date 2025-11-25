using Vogen;

namespace Enrollify.Core.UserAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct UserEmail
{
    public const int MaxLength = 255;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Email cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
