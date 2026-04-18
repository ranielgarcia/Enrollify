using Enrollify.Core.Validators;
using Vogen;

namespace Enrollify.Core.Aggregates.UserAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct UserEmail
{
    public const int MaxLength = 255;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("User email cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"User email cannot exceed {MaxLength} characters");

        if (!EmailValidator.IsValid(value))
            return Validation.Invalid("User email is not a valid email address");

        return Validation.Ok;
    }
}
