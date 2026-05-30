using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class DeleteSubjectOfferingRequest
{
    public int Id { get; set; }
}

public class DeleteSubjectOfferingRequestValidator : Validator<DeleteSubjectOfferingRequest>
{
    public DeleteSubjectOfferingRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid offering ID is required.");
    }
}

[HttpDelete("{id:int}")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteSubjectOfferingPermission)]
public class DeleteSubjectOfferingEndpoint : Endpoint<DeleteSubjectOfferingRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public DeleteSubjectOfferingEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult> ExecuteAsync(
        DeleteSubjectOfferingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteClassSectionSubjectOffering.Command(
                ClassSectionSubjectOfferingId.From(request.Id)),
            cancellationToken);

        return result.ToDeleteResult();
    }
}
