using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSections.EventHandlers;

/// <summary>
/// Handles <see cref="ClassSectionEligibilityRecomputeRequestedEvent"/> — re-runs the full
/// eligibility validation pipeline and replaces all stored messages for the affected section.
/// Triggered by adviser updates on <see cref="ClassSection"/> and by teacher, room, or schedule
/// changes on <see cref="ClassSectionSubjectOffering"/>.
/// </summary>
public sealed class ClassSectionEligibilityRecomputeRequestedEventHandler(
  IReadRepository<ClassSection> classSectionReadRepository,
  IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
  ClassSectionOpenForEnrollmentEligibilityValidationPipeline pipeline,
  IClassSectionEligibilityValidationMessageRepository messageRepository,
  ILogger<ClassSectionEligibilityRecomputeRequestedEventHandler> logger)
  : IDomainEventHandler<ClassSectionEligibilityRecomputeRequestedEvent>
{
  public async Task Handle(ClassSectionEligibilityRecomputeRequestedEvent notification,
    CancellationToken cancellationToken)
  {
    ClassSectionId classSectionId = notification.ClassSectionId;

    ClassSection? section = await classSectionReadRepository.GetByIdAsync(classSectionId, cancellationToken);
    if (section is null)
    {
      logger.LogWarning(
        "ClassSectionEligibilityRecomputeRequestedEventHandler: ClassSection {ClassSectionId} not found — skipping",
        classSectionId.Value);
      return;
    }

    List<ClassSectionSubjectOffering> offerings = await offeringReadRepository.ListAsync(
      new GetClassSectionSubjectOfferingsByClassSectionIdSpec(classSectionId), cancellationToken);

    ClassSectionOpenForEnrollmentEligibilityValidationContext context =
      pipeline.Validate(new ClassSectionOpenForEnrollmentEligibilityValidationContext(section, offerings));

    var classSectionValidationErrors = context.ValidationErrors
      .Select(e => new ClassSectionEnrollmentEligibilityValidationMessage(classSectionId, null, e.Code, e.Message))
      .ToList();

    var offeringValidationErrors = context.OfferingValidationErrors
      .SelectMany(kvp => kvp.Value.Select(e => new ClassSectionEnrollmentEligibilityValidationMessage(
        classSectionId, kvp.Key, e.Code, e.Message)))
      .ToList();

    var informationalMessages = context.InformationalMessages
      .Select(e => new ClassSectionEnrollmentEligibilityValidationMessage(classSectionId, null, e.Code, e.Message))
      .ToList();

    var softRuleMessages = context.SoftRulesMessages
      .Select(e => new ClassSectionEnrollmentEligibilityValidationMessage(classSectionId, null, e.Code, e.Message))
      .ToList();

    var messages =
      classSectionValidationErrors.Concat(offeringValidationErrors).Concat(informationalMessages)
        .Concat(softRuleMessages)
        .ToList();

    await messageRepository.ReplaceAllForSectionAsync(classSectionId, messages, cancellationToken);

    logger.LogInformation(
      "Eligibility recomputed for ClassSection {ClassSectionId}: {ErrorCount} error(s)",
      classSectionId.Value, context.ValidationErrors.Count);
  }
}
