using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Core.Aggregates.ClassSectionConflictAggregate.Models;

public record AffectedOffering
{
  public ClassSectionSubjectOfferingId Id { get; init; }
  public SubjectSummary Subject { get; init; } = null!;
  public SectionSummary Section { get; init; } = null!;
  public RoomSummary? Room { get; init; }
}

public record SubjectSummary(SubjectCode Code, string Title);
public record SectionSummary(ClassSectionId Id, string Name);
public record RoomSummary(string RoomNumber, string Building);

