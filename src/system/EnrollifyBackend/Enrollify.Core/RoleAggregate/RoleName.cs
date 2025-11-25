using Vogen;

namespace Enrollify.Core.RoleAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct RoleName
{
    public const int MaxLength = 50;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Role name cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Role name cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}
