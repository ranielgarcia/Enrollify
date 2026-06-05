using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class RemoveScheduleFromOfferingRequest
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
}

public class RemoveScheduleFromOfferingRequestValidator : Validator<RemoveScheduleFromOfferingRequest>
{
    public RemoveScheduleFromOfferingRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid offering ID is required.");

        RuleFor(x => x.ScheduleId)
            .GreaterThan(0).WithMessage("A valid schedule ID is required.");
    }
}

[HttpDelete("{id:int}/schedules/{scheduleId:int}")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectOfferingPermission)]
public class RemoveScheduleFromOfferingEndpoint : Endpoint<RemoveScheduleFromOfferingRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public RemoveScheduleFromOfferingEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult> ExecuteAsync(
        RemoveScheduleFromOfferingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveScheduleFromOffering.Command(
                ClassSectionSubjectOfferingId.From(request.Id),
                ClassScheduleId.From(request.ScheduleId)),
            cancellationToken);

        return result.ToDeleteResult();
    }
}
