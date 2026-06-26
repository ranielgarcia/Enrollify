namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public enum BulkOperationStatus
{
  Success,
  PartialSuccess,
  Failed
}

public sealed record BulkOpenClassSectionsResultDto
{
  public BulkOperationStatus Status { get; init; }
  public int TotalRequested { get; init; }
  public int Succeeded { get; init; }
  public int Failed { get; init; }
  public Dictionary<string, string> Errors { get; init; } = new();
}
