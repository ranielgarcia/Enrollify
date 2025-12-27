using Vogen;

namespace Enrollify.Core.Aggregates.CourseAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct CourseCode
{
    public const int MaxLength = 10;
    private static Validation Validate(in string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Validation.Invalid("Course code cannot be empty.");
        }
        if (name.Length > MaxLength)
        {
            return Validation.Invalid($"Course code cannot exceed {MaxLength} characters.");
        }
        return Validation.Ok;
    }
}
