using Enrollify.Application.Features.ClassSectionSubjectOfferings.DTOs;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Queries;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class GetOfferingsByClassSectionIdRequest
{
    [QueryParam] public int SectionId { get; set; }
}

public class GetOfferingsByClassSectionIdRequestValidator : Validator<GetOfferingsByClassSectionIdRequest>
{
    public GetOfferingsByClassSectionIdRequestValidator()
    {
        RuleFor(x => x.SectionId)
            .GreaterThan(0).WithMessage("A valid section ID is required.");
    }
}

[HttpGet("")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectOfferingsPermission)]
public class GetOfferingsByClassSectionIdEndpoint
    : Endpoint<GetOfferingsByClassSectionIdRequest, Ok<IReadOnlyList<ClassSectionSubjectOfferingDto>>>
{
    private readonly IMediator _mediator;

    public GetOfferingsByClassSectionIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Ok<IReadOnlyList<ClassSectionSubjectOfferingDto>>> ExecuteAsync(
        GetOfferingsByClassSectionIdRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetOfferingsByClassSectionIdQuery(ClassSectionId.From(request.SectionId)),
            cancellationToken);

        return result.ToOkOnlyResult(dtos => dtos);
    }
}
