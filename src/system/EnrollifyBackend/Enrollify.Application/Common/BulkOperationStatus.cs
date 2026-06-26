using Ardalis.SmartEnum;

namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public sealed class BulkOperationStatus : SmartEnum<BulkOperationStatus>
{
  public static readonly BulkOperationStatus Success = new("Success", 1);
  public static readonly BulkOperationStatus PartialSuccess = new("PartialSuccess", 1);
  public static readonly BulkOperationStatus Failed = new("Failed", 1);

  public BulkOperationStatus(string name, int value) :  base(name, value)
  {
  }
}
