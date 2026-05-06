namespace Enrollify.Core.DomainExceptions;

public class InvalidAcademicYearRangeException : Exception
{
    public InvalidAcademicYearRangeException()
        : base("The end year must be exactly one year after the start year.")
    {
    }

    public InvalidAcademicYearRangeException(string message)
        : base(message)
    {
    }

    public InvalidAcademicYearRangeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
