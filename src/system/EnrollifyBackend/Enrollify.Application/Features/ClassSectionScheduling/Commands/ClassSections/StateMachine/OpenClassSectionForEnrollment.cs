using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;
using MediatR;

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

    public Handler(
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionRepository classSectionRepository,
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IMediator mediator)
    {
      _classSectionReadRepository = classSectionReadRepository;
      _classSectionRepository = classSectionRepository;
      _offeringReadRepository = offeringReadRepository;
      _mediator = mediator;
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
          "Cannot open a class section for enrollment with no subject offerings."));

      bool hasValidationErrors = await HasValidationIssues(section.Id, cancellationToken);
      if (hasValidationErrors)
        return Result.Invalid(new ValidationError("EnrollmentEligibilityCheckFailed",
          "This class section is not eligible for enrollment."));

      try
      {
        section.OpenForEnrollment();
        Result<ClassSectionId> result = await _classSectionRepository.Update(section, cancellationToken);

        return result.IsSuccess
          ? Result.Success(result.Value)
          : Result.Error(string.Join("; ", result.Errors));
      }
      catch (ArgumentException ex)
      {
        return Result.Invalid(new ValidationError(ex.Message));
      }
    }

    private async Task<bool> HasValidationIssues(ClassSectionId sectionId, CancellationToken cancellationToken)
    {
      Result<List<ClassSectionValidationIssueDto>> validationIssues =
        await _mediator.Send(new ComputeAndGetValidationIssuesForClassSection.Command(sectionId),
          cancellationToken);
      return validationIssues.IsSuccess && validationIssues.Value
        .Where(x => x.Type.Tier != ClassSectionValidationIssueTierEnum.INFORMATIONAL).ToList().Count > 0;
    }
  }
}
