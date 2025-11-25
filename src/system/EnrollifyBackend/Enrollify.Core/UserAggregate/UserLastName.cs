using Vogen;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct UserLastName
{
    public const int MaxLength = 50;
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Lastname cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Lastname cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }
}