namespace Enrollify.Core.DomainExceptions;

public class InvalidSubjectOfferingForClassSectionException : Exception
{
  public InvalidSubjectOfferingForClassSectionException()
  {
  }

  public InvalidSubjectOfferingForClassSectionException(string? message) : base(message)
  {
  }

  public InvalidSubjectOfferingForClassSectionException(string? message, Exception? innerException) : base(message,
    innerException)
  {
  }
}
