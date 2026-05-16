namespace Enrollify.Core.DomainExceptions;

public class InvalidClassScheduleException : Exception
{
    public InvalidClassScheduleException()
    {
    }

    public InvalidClassScheduleException(string? message) : base(message)
    {
    }

    public InvalidClassScheduleException(string? message, Exception? innerException) : base(message, innerException)
    {
        
    }
}
