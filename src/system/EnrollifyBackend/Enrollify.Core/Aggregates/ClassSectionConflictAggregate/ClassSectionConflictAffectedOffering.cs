using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Core.Aggregates.ClassSectionConflictAggregate;

public class ClassSectionConflictAffectedOffering
{
  private ClassSectionConflictAffectedOffering() { }

  public ClassSectionConflictAffectedOffering(ClassSectionConflictId conflictId, ClassSectionSubjectOfferingId offeringId)
  {
    ConflictId = conflictId;
    OfferingId = offeringId;
  }

  public ClassSectionConflictAffectedOfferingId Id { get; private set; }
  public ClassSectionConflictId ConflictId { get; private set; }
  public ClassSectionSubjectOfferingId OfferingId { get; private set; }
}
