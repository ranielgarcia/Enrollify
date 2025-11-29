using Vogen;

namespace Enrollify.Core.Aggregates.RoleAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct RoleDescription
{
    public const int MaxLength = 255;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Role description cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Role description cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}

