using Enrollify.Core.Validators;
using Vogen;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct TeacherEmail
{
    public const int MaxLength = 255;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Teacher email cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Teacher email cannot exceed {MaxLength} characters");

        if (!EmailValidator.IsValid(value))
            return Validation.Invalid("Teacher email is not a valid email address");

        return Validation.Ok;
    }
}
