using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class AddScheduleToOfferingRequest
{
    public int Id { get; set; }
    public string DayOfWeek { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public class AddScheduleToOfferingRequestValidator : Validator<AddScheduleToOfferingRequest>
{
    public AddScheduleToOfferingRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid offering ID is required.");

        RuleFor(x => x.DayOfWeek)
            .NotEmpty().WithMessage("Day of week is required.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
    }
}

[HttpPost("{id:int}/schedules")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectOfferingPermission)]
public class AddScheduleToOfferingEndpoint : Endpoint<AddScheduleToOfferingRequest, CreatedApiResult<int>>
{
    private readonly IMediator _mediator;

    public AddScheduleToOfferingEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<int>> ExecuteAsync(
        AddScheduleToOfferingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AddScheduleToOffering.Command(
                ClassSectionSubjectOfferingId.From(request.Id),
                request.DayOfWeek,
                request.StartTime,
                request.EndTime),
            cancellationToken);

        return result.ToCreatedResult(
            scheduleId => $"/subject-offerings/{request.Id}/schedules/{scheduleId.Value}",
            scheduleId => scheduleId.Value);
    }
}
