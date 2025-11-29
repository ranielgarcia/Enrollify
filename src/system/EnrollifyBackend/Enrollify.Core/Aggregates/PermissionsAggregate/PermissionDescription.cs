using Vogen;

namespace Enrollify.Core.Aggregates.PermissionsAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct PermissionDescription
{
    public const int MaxLength = 255;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Description cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Description cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
