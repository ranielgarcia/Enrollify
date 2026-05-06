using Enrollify.Application.Features.AcademicYearAndTerm.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

public class DeleteAcademicYearAndTermsRequest
{
    public int Id { get; set; }
}

public class DeleteAcademicYearAndTermsRequestValidator : Validator<DeleteAcademicYearAndTermsRequest>
{
    public DeleteAcademicYearAndTermsRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid academic year ID.");
    }
}

[HttpDelete("{id:int}")]
[Group<AcademicYearAndTermEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteAcademicYearAndTermPermission)]
public class DeleteAcademicYearAndTermsEndpoint : Endpoint<DeleteAcademicYearAndTermsRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public DeleteAcademicYearAndTermsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult>
        ExecuteAsync(DeleteAcademicYearAndTermsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(request.Id)), ct);

        return result.ToDeleteResult();
    }
}
