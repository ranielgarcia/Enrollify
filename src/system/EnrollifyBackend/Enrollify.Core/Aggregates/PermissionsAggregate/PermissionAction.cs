using Vogen;

namespace Enrollify.Core.Aggregates.PermissionsAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct PermissionAction
{
    public const int MaxLength = 50;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Action cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Action cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
