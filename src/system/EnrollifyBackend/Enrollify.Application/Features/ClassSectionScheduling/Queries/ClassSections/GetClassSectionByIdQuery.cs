using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionValidationIssues;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSections;

public record GetClassSectionByIdQuery(ClassSectionId Id) : IRequest<Result<ClassSectionDetailDto>>;

public class GetClassSectionByIdQueryHandler
  : IRequestHandler<GetClassSectionByIdQuery, Result<ClassSectionDetailDto>>
{
  private readonly IReadRepository<ClassSection> _classSectionReadRepository;
  private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
  private readonly IReadRepository<ClassSectionValidationIssue> _validationIssueReadRepository;

  public GetClassSectionByIdQueryHandler(
    IReadRepository<ClassSection> classSectionReadRepository,
    IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
    IReadRepository<ClassSectionValidationIssue> validationIssueReadRepository)
  {
    _classSectionReadRepository = classSectionReadRepository;
    _offeringReadRepository = offeringReadRepository;
    _validationIssueReadRepository = validationIssueReadRepository;
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
      new GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(section.Id), cancellationToken);

    List<ClassSectionValidationIssue> validationIssues =
      await _validationIssueReadRepository.ListAsync(new GetClassSectionValidationIssuesByClassSectionId(section.Id),
        cancellationToken);

    return Result.Success(ClassSectionDetailDto.FromEntities(section, offerings, validationIssues));
  }
}
