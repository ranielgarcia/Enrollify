namespace Enrollify.Core.DomainExceptions;

public class InvalidAcademicTermException : Exception
{
    public InvalidAcademicTermException()
    {
    }

    public InvalidAcademicTermException(string? message) : base(message)
    {
    }

    public InvalidAcademicTermException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
