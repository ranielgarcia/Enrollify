using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

/// <summary>
/// Represents a detected scheduling conflict.
/// Matches the frontend ClassScheduleConflictResult type in offering.ts.
/// Used in both API responses and section detail queries.
/// </summary>
public record ClassSectionValidationIssueDto
{
  public string? Id { get; init; }

  public string Type { get; init; }

  public string Severity { get; init; }

  public string Message { get; init; } = string.Empty;

  public string? DayOfWeek { get; init; }

  public string? StartTime { get; init; }
  public string? EndTime { get; init; }

  public List<ConflictingOfferingDto>? ConflictingOfferings { get; init; }

  public static ClassSectionValidationIssueDto FromEntity(ClassSectionValidationIssue issue)
  {
    return new ClassSectionValidationIssueDto
    {
      Type = issue.Type.Name,
      Severity = issue.Severity.Name,
      Message = issue.Message,
      DayOfWeek = issue.DayOfWeek.Value,
      StartTime = issue.StartTime?.ToString("HH:mm:ss"),
      EndTime = issue.EndTime?.ToString("HH:mm:ss"),
      ConflictingOfferings = issue.ConflictingOfferings?.Select(a => new ConflictingOfferingDto
      {
        Id = a.Id.Value,
        Subject = new SubjectSummaryDto(a.Subject.Code.Value, a.Subject.Title),
        Section = new SectionSummaryDto(a.Section.Id.Value, a.Section.Name),
        Room = a.Room != null
          ? new RoomSummaryDto(a.Room.RoomNumber, a.Room.Building)
          : null
      }).ToList()
    };
  }
}

/// <summary>
/// Summary of an offering affected by a conflict
/// </summary>
public record ConflictingOfferingDto
{
  public int Id { get; init; }
  public SubjectSummaryDto Subject { get; init; } = null!;
  public SectionSummaryDto Section { get; init; } = null!;
  public RoomSummaryDto? Room { get; init; }
}

public record SubjectSummaryDto(string Code, string Title);

public record SectionSummaryDto(int Id, string Name);

public record RoomSummaryDto(string RoomNumber, string Building);
