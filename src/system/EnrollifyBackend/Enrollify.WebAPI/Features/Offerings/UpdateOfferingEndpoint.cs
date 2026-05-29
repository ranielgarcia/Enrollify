using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.WebAPI.Features.Offerings;

public class UpdateOfferingResponse
{
    public int Id { get; set; }
    public int? MaxNumberOfStudents { get; set; }
}

public class UpdateOfferingRequest
{
    public int Id { get; set; }
    public int? MaxNumberOfStudents { get; set; }
}

public class UpdateOfferingRequestValidator : Validator<UpdateOfferingRequest>
{
    public UpdateOfferingRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid offering ID.");

        When(x => x.MaxNumberOfStudents.HasValue, () =>
        {
            RuleFor(x => x.MaxNumberOfStudents!.Value)
                .GreaterThan(0).WithMessage("Max number of students must be greater than zero.");
        });
    }
}

[HttpPut("{id:int}")]
[Group<OfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class UpdateOfferingEndpoint : Endpoint<UpdateOfferingRequest, OkOrNotFoundApiResult<UpdateOfferingResponse>>
{
    private readonly IMediator _mediator;

    public UpdateOfferingEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OkOrNotFoundApiResult<UpdateOfferingResponse>> ExecuteAsync(
        UpdateOfferingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateClassSectionSubjectOfferingMaxStudents.Command(
                ClassSectionSubjectOfferingId.From(request.Id),
                request.MaxNumberOfStudents),
            cancellationToken);

        return result.ToUpdateResult(id => new UpdateOfferingResponse
        {
            Id = id.Value,
            MaxNumberOfStudents = request.MaxNumberOfStudents,
        });
    }
}
