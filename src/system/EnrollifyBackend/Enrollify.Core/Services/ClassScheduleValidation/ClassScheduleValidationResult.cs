namespace Enrollify.Core.Services.ClassScheduleValidation;

/// <summary>
/// Represents the result of a class schedule validation operation.
/// </summary>
public class ClassScheduleValidationResult
{
  public bool IsValid { get; }
  public string ErrorMessage { get; }

  private ClassScheduleValidationResult(bool isValid, string errorMessage = "")
  {
    IsValid = isValid;
    ErrorMessage = errorMessage;
  }

  /// <summary>
  /// Creates a successful validation result.
  /// </summary>
  public static ClassScheduleValidationResult Success()
  {
    return new ClassScheduleValidationResult(true);
  }

  /// <summary>
  /// Creates a failed validation result with an error message.
  /// </summary>
  public static ClassScheduleValidationResult Failure(string errorMessage)
  {
    return new ClassScheduleValidationResult(false, errorMessage);
  }
}
