using Enrollify.Core.Validators;
using Vogen;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct TeacherPhoneNumber
{
    public const int MaxLength = 11;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Teacher phone number cannot be empty");
        if (value.Length > MaxLength)
            return Validation.Invalid($"Teacher phone number cannot exceed {MaxLength} characters");

        if (!PhoneNumberValidator.IsValid(value))
            return Validation.Invalid("Teacher phone number is not a valid phone number");

        return Validation.Ok;
    }
}
