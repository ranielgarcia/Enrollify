using System.Text.RegularExpressions;
using Vogen;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct TeacherIdentifier
{
    public const int MaxLength = 20;

    //[GeneratedRegex(@"^[a-zA-Z0-9\-]+$")]
    //private static partial Regex AlphanumericPattern();

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Teacher identifier number cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Teacher identifier number cannot exceed {MaxLength} characters");

        //if (!AlphanumericPattern().IsMatch(value))
        //    return Validation.Invalid("Teacher identifier number must contain only alphanumeric characters and hyphens");

        return Validation.Ok;
    }
}
