using Vogen;

namespace Enrollify.Core.Aggregates.DepartmentAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct DepartmentCode
{
    public const int MaxLength = 10;
    private static Validation Validate(in string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Validation.Invalid("Department code cannot be empty.");
        }
        if (name.Length > MaxLength)
        {
            return Validation.Invalid($"Department code cannot exceed {MaxLength} characters.");
        }
        return Validation.Ok;
    }
}
