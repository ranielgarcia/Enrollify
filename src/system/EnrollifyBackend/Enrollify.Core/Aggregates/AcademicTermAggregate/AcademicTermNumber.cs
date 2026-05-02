using Vogen;

namespace Enrollify.Core.Aggregates.AcademicTermAggregate;

[ValueObject<int>]
public readonly partial struct AcademicTermNumber
{
    public const int MaxTermNumber = 3;
    private static Validation Validate(int termNumber)
    {
        if (termNumber >  MaxTermNumber)
            return Validation.Invalid($"Term number cannot exceed {MaxTermNumber}");

        if (termNumber < 1)
            return Validation.Invalid($"Term number must be between 1 and {MaxTermNumber}");

        return Validation.Ok;
    }
}
