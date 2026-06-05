using Ardalis.Result;
using Enrollify.Application.Features.ClassSections.DTOs;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSections.Queries;

public record GetClassSectionByIdQuery(ClassSectionId Id) : IRequest<Result<ClassSectionDetailDto>>;

public class GetClassSectionByIdQueryHandler
  : IRequestHandler<GetClassSectionByIdQuery, Result<ClassSectionDetailDto>>
{
  private readonly IReadRepository<ClassSection> _classSectionReadRepository;
  private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
  private readonly IReadRepository<ClassSectionEnrollmentEligibilityValidationMessage> _validationMessageRepository;

  public GetClassSectionByIdQueryHandler(
    IReadRepository<ClassSection> classSectionReadRepository,
    IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
    IReadRepository<ClassSectionEnrollmentEligibilityValidationMessage> validationMessageRepository)
  {
    _classSectionReadRepository = classSectionReadRepository;
    _offeringReadRepository = offeringReadRepository;
    _validationMessageRepository = validationMessageRepository;
  }

  public async Task<Result<ClassSectionDetailDto>> Handle(
    GetClassSectionByIdQuery request,
    CancellationToken cancellationToken)
  {
    ClassSection? section = await _classSectionReadRepository.FirstOrDefaultAsync(
      new GetClassSectionFullDetailsByIdSpec(request.Id), cancellationToken);

    if (section is null)
      return Result.NotFound($"Class section with id {request.Id.Value} was not found.");

    List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
      new GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(request.Id), cancellationToken);

    List<ClassSectionEnrollmentEligibilityValidationMessage> allValidationMessagesForSection =
      await _validationMessageRepository
        .ListAsync(new GetClassSectionEnrollmentEligibilityValidationMessagesSpec(section.Id), cancellationToken);

    return Result.Success(ClassSectionDetailDto.FromEntities(section, offerings,
      allValidationMessagesForSection));
  }
}
