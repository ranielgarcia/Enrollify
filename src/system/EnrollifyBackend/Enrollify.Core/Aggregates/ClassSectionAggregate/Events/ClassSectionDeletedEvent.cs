using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

public class ClassSectionDeletedEvent : DomainEventBase
{
  public ClassSectionId ClassSectionId { get; }

  public ClassSectionDeletedEvent(ClassSectionId classSectionId)
  {
    ClassSectionId = classSectionId;
  }
}
