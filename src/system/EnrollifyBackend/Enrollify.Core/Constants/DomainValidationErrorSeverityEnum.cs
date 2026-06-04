using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class DomainValidationErrorSeverityEnum : SmartEnum<DomainValidationErrorSeverityEnum>
{
  public static readonly DomainValidationErrorSeverityEnum Info = new("Info", 1);
  public static readonly DomainValidationErrorSeverityEnum Warning = new("Warning", 2);
  public static readonly DomainValidationErrorSeverityEnum Error = new("Error", 3);

  public DomainValidationErrorSeverityEnum(string name, int value) : base(name, value)
  {
  }
}
