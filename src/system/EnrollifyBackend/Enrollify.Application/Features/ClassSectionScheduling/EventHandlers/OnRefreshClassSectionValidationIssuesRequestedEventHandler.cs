using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

/// <summary>
/// Handles <see cref="RefreshClassSectionValidationIssuesRequestedEvent"/> — re-runs the full
/// eligibility validation pipeline and replaces all stored messages for the affected section.
/// Triggered by adviser updates on <see cref="ClassSection"/> and by teacher, room, or schedule
/// changes on <see cref="ClassSectionSubjectOffering"/>.
/// </summary>
public sealed class OnRefreshClassSectionValidationIssuesRequestedEventHandler(
  IMediator mediator)
  : IDomainEventHandler<RefreshClassSectionValidationIssuesRequestedEvent>
{
  public async Task Handle(RefreshClassSectionValidationIssuesRequestedEvent notification,
    CancellationToken cancellationToken)
  {
    ClassSectionId classSectionId = notification.ClassSectionId;

    await mediator.Send(new ComputeAndGetValidationIssuesForClassSection.Command(classSectionId),
      cancellationToken);
  }
}
