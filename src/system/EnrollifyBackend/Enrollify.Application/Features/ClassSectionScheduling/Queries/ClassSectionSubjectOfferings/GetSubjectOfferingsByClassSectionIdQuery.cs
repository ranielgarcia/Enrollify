using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionValidationIssues;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSectionSubjectOfferings;

public record GetSubjectOfferingsByClassSectionIdQuery(ClassSectionId SectionId)
  : IRequest<Result<List<ClassSectionSubjectOfferingDto>>>;

public class GetSubjectOfferingsByClassSectionIdQueryHandler
  : IRequestHandler<GetSubjectOfferingsByClassSectionIdQuery, Result<List<ClassSectionSubjectOfferingDto>>>
{
  private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
  private readonly IReadRepository<ClassSectionValidationIssue> _validationIssueReadRepository;

  public GetSubjectOfferingsByClassSectionIdQueryHandler(
    IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
    IReadRepository<ClassSectionValidationIssue> validationIssueReadRepository)
  {
    _offeringReadRepository = offeringReadRepository;
    _validationIssueReadRepository = validationIssueReadRepository;
  }

  public async Task<Result<List<ClassSectionSubjectOfferingDto>>> Handle(
    GetSubjectOfferingsByClassSectionIdQuery request,
    CancellationToken cancellationToken)
  {
    List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
      new GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(request.SectionId), cancellationToken);

    List<ClassSectionValidationIssue> validationIssues =
      await _validationIssueReadRepository.ListAsync(
        new GetClassSectionValidationIssuesByClassSectionId(request.SectionId),
        cancellationToken);

    var offeringValidationIssuesLookup = validationIssues
      .Where(x => x.OfferingId != null)
      .GroupBy(x => x.OfferingId!.Value)
      .ToDictionary(g => g.Key, g => g.ToList());

    var offeringDtos = offerings.Select(o =>
    {
      List<ClassSectionValidationIssue> offeringValidationIssues =
        offeringValidationIssuesLookup.TryGetValue(o.Id, out List<ClassSectionValidationIssue>? issues)
          ? issues
          : new List<ClassSectionValidationIssue>();
      return ClassSectionSubjectOfferingDto.FromEntity(o, offeringValidationIssues);
    }).ToList();

    return Result.Success(offeringDtos);
  }
}
