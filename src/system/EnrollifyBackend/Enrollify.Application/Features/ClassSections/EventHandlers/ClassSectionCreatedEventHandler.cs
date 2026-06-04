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
/// Handles <see cref="ClassSectionCreatedEvent"/> — computes the initial eligibility
/// validation messages for a newly created class section.
/// </summary>
public sealed class ClassSectionCreatedEventHandler(
  IReadRepository<ClassSection> classSectionReadRepository,
  IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
  ClassSectionOpenForEnrollmentEligibilityValidationPipeline pipeline,
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
      pipeline.Validate(new ClassSectionOpenForEnrollmentEligibilityValidationContext(section, offerings));

    IEnumerable<ClassSectionEnrollmentEligibilityValidationMessage> messages = context.ValidationErrors
      .Select(e => new ClassSectionEnrollmentEligibilityValidationMessage(classSectionId, e.Code, e.Message));

    await messageRepository.ReplaceAllForSectionAsync(classSectionId, messages, cancellationToken);

    logger.LogInformation(
      "Eligibility recomputed for ClassSection {ClassSectionId}: {ErrorCount} error(s)",
      classSectionId.Value, context.ValidationErrors.Count);
  }
}

