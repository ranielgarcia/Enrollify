using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

/// <summary>
/// Handles <see cref="ClassSectionCreatedEvent"/> — computes the initial eligibility
/// validation messages for a newly created class section.
/// </summary>
public sealed class ClassSectionCreatedEventHandler(
  IReadRepository<ClassSection> classSectionReadRepository,
  IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
  IClassSectionEligibilityValidationMessageRepository messageRepository,
  ILogger<ClassSectionCreatedEventHandler> logger)
  : IDomainEventHandler<ClassSectionCreatedEvent>
{
  public async Task Handle(ClassSectionCreatedEvent notification, CancellationToken cancellationToken)
  {
    await RecomputeAsync(notification.Id, cancellationToken);
  }

  private async Task RecomputeAsync(ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    ClassSection? section = await classSectionReadRepository.GetByIdAsync(classSectionId, cancellationToken);
    if (section is null)
    {
      logger.LogWarning(
        "ClassSectionCreatedEventHandler: ClassSection {ClassSectionId} not found — skipping eligibility recompute",
        classSectionId.Value);
      return;
    }

    List<ClassSectionSubjectOffering> offerings = await offeringReadRepository.ListAsync(
      new GetClassSectionSubjectOfferingsByClassSectionIdSpec(classSectionId), cancellationToken);

    ClassSectionOpenForEnrollmentEligibilityValidationContext context =
      ClassSectionEnrollmentEligibilityValidator.Validate(section, offerings);

    var classSectionValidationErrors = context.ClassSectionValidationMessages
      .Select(e =>
        new ClassSectionEnrollmentEligibilityValidationMessage(e.Severity, classSectionId, null, e.Code, e.Message))
      .ToList();

    var offeringValidationErrors = context.OfferingValidationMessages
      .SelectMany(kvp => kvp.Value.Select(e => new ClassSectionEnrollmentEligibilityValidationMessage(
        e.Severity, classSectionId, kvp.Key, e.Code, e.Message)))
      .ToList();

    var messages =
      classSectionValidationErrors.Concat(offeringValidationErrors)
        .ToList();

    await messageRepository.ReplaceAllForSectionAsync(classSectionId, messages, cancellationToken);

    logger.LogInformation(
      "Eligibility recomputed for ClassSection {ClassSectionId}: {ErrorCount} error(s)",
      classSectionId.Value, messages.Count);
  }
}
