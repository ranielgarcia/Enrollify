using Vogen;

namespace Enrollify.Core.Aggregates.PermissionsAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct PermissionName
{
    public const int MaxLength = 100;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Role Permission name cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Role Permission name cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
