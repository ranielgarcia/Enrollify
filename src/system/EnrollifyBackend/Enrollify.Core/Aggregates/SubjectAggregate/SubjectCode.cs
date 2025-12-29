using Vogen;

namespace Enrollify.Core.Aggregates.SubjectAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct SubjectCode
{
    public const int MaxLength = 10;
    private static Validation Validate(in string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Validation.Invalid("Subject code cannot be empty.");
        }
        if (name.Length > MaxLength)
        {
            return Validation.Invalid($"Subject code cannot exceed {MaxLength} characters.");
        }
        return Validation.Ok;
    }
}
