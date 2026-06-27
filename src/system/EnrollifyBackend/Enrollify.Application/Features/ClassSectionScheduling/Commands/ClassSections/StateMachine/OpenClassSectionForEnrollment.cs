using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;

public static class OpenClassSectionForEnrollment
{
  public sealed record Command(ClassSectionId Id) : IRequest<Result<ClassSectionId>>;

  public sealed class Handler : IRequestHandler<Command, Result<ClassSectionId>>
  {
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionRepository classSectionRepository,
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IMediator mediator,
      ILogger<Handler> logger)
    {
      _classSectionReadRepository = classSectionReadRepository;
      _classSectionRepository = classSectionRepository;
      _offeringReadRepository = offeringReadRepository;
      _mediator = mediator;
      _logger = logger;
    }

    public async Task<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
    {
      ClassSection? section = await _classSectionReadRepository.GetByIdAsync(command.Id, cancellationToken);
      if (section is null)
        return Result.NotFound($"Class section with ID {command.Id.Value} not found.");

      int offeringsCount = await _offeringReadRepository.CountAsync(
        new GetMinimalClassSectionSubjectOfferingsByClassSectionIdSpec(command.Id), cancellationToken);

      if (offeringsCount == 0)
        return Result.Invalid(new ValidationError("EnrollmentEligibilityCheckFailed",
          "Cannot open a class section for enrollment without any subject offerings. Please add at least one subject offering before opening for enrollment."));

      bool hasValidationErrors = await HasValidationIssues(section.Id, cancellationToken);
      if (hasValidationErrors)
        return Result.Invalid(new ValidationError("EnrollmentEligibilityCheckFailed",
          "This class section is not eligible for enrollment due to validation issues. Please resolve the issues before opening for enrollment."));

      try
      {
        section.OpenForEnrollment();
        Result<ClassSectionId> result = await _classSectionRepository.Update(section, cancellationToken);

        return result.IsSuccess
          ? Result.Success(result.Value)
          : Result.Error(string.Join("; ", result.Errors));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, ex.Message);
        return Result.Invalid(new ValidationError(ex.Message));
      }
    }

    private async Task<bool> HasValidationIssues(ClassSectionId sectionId, CancellationToken cancellationToken)
    {
      Result<List<ClassSectionValidationIssue>> validationIssues =
        await _mediator.Send(new ComputeAndGetValidationIssuesForClassSection.Command(sectionId),
          cancellationToken);
      return validationIssues.IsSuccess && validationIssues.Value
        .Where(x => x.Type.Category != ClassSectionValidationIssueCategoryEnum.DEFAULT_VALUE).ToList().Count > 0;
    }
  }
}
