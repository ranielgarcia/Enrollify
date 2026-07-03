namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public sealed record BulkStateChangeClassSectionsResultDto
{
  public string Status { get; init; } = null!;
  public int TotalRequested { get; init; }
  public int Succeeded { get; init; }
  public int Failed { get; init; }
  public Dictionary<string, string> Errors { get; init; } = new();
}
