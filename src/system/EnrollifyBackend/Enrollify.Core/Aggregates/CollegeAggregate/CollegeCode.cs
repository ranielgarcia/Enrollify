using Vogen;

namespace Enrollify.Core.Aggregates.CollegeAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct CollegeCode
{
    public const int MaxLength = 10;
    private static Validation Validate(in string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Validation.Invalid("College code cannot be empty.");
        }
        if (name.Length > MaxLength)
        {
            return Validation.Invalid($"College code cannot exceed {MaxLength} characters.");
        }
        return Validation.Ok;
    }
}
