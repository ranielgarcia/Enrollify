using Vogen;

namespace Enrollify.Core.Aggregates.PermissionsAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct PermissionResource
{
    public const int MaxLength = 50;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Resource cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Resource cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
