using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.DTOs;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Queries;

public record GetOfferingsByClassSectionIdQuery(ClassSectionId ClassSectionId)
    : IRequest<Result<IReadOnlyList<ClassSectionSubjectOfferingDto>>>;

public class GetOfferingsByClassSectionIdQueryHandler
    : IRequestHandler<GetOfferingsByClassSectionIdQuery, Result<IReadOnlyList<ClassSectionSubjectOfferingDto>>>
{
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;

    public GetOfferingsByClassSectionIdQueryHandler(
        IReadRepository<ClassSectionSubjectOffering> offeringReadRepository)
    {
        _offeringReadRepository = offeringReadRepository;
    }

    public async Task<Result<IReadOnlyList<ClassSectionSubjectOfferingDto>>> Handle(
        GetOfferingsByClassSectionIdQuery request,
        CancellationToken cancellationToken)
    {
        List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
            new GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(request.ClassSectionId),
            cancellationToken);

        IReadOnlyList<ClassSectionSubjectOfferingDto> dtos = offerings
            .Select(ClassSectionSubjectOfferingDto.FromEntity)
            .ToList()
            .AsReadOnly();

        return Result.Success(dtos);
    }
}
