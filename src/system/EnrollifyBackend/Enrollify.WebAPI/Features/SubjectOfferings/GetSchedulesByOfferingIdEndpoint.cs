using Enrollify.Application.Features.ClassSectionSubjectOfferings.DTOs;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

[HttpGet("{id:int}/schedules")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectOfferingsPermission)]
public class GetSchedulesByOfferingIdEndpoint : EndpointWithoutRequest<OkOrNotFoundApiResult<IReadOnlyList<ClassScheduleDto>>>
{
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;

    public GetSchedulesByOfferingIdEndpoint(IReadRepository<ClassSectionSubjectOffering> offeringReadRepository)
    {
        _offeringReadRepository = offeringReadRepository;
    }

    public override async Task<OkOrNotFoundApiResult<IReadOnlyList<ClassScheduleDto>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        int id = Route<int>("id");
        var offeringId = ClassSectionSubjectOfferingId.From(id);

        ClassSectionSubjectOffering? offering = await _offeringReadRepository.FirstOrDefaultAsync(
            new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(offeringId), cancellationToken);

        if (offering is null)
            return TypedResults.NotFound();

        IReadOnlyList<ClassScheduleDto> schedules = offering.ClassSchedules
            .Select(ClassScheduleDto.FromEntity)
            .ToList()
            .AsReadOnly();

        return TypedResults.Ok(schedules);
    }
}
