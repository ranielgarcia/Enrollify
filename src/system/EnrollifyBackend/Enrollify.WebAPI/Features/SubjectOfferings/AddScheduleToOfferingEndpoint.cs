using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class ScheduleItem
{
    public string DayOfWeek { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public class AddMultipleSchedulesToOfferingRequest
{
    public int Id { get; set; }
    public List<ScheduleItem> Schedules { get; set; } = new();
}

public class AddMultipleSchedulesToOfferingResponse
{
    public List<int> ScheduleIds { get; set; } = new();
}

public class ScheduleItemValidator : Validator<ScheduleItem>
{
    public ScheduleItemValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .NotEmpty().WithMessage("Day of week is required.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
    }
}

public class AddMultipleSchedulesToOfferingRequestValidator : Validator<AddMultipleSchedulesToOfferingRequest>
{
    public AddMultipleSchedulesToOfferingRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid offering ID is required.");

        RuleFor(x => x.Schedules)
            .NotEmpty().WithMessage("At least one schedule is required.");

        RuleForEach(x => x.Schedules)
            .SetValidator(new ScheduleItemValidator());
    }
}

[HttpPost("{id:int}/schedules")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectOfferingPermission)]
public class AddMultipleSchedulesToOfferingEndpoint : Endpoint<AddMultipleSchedulesToOfferingRequest, CreatedApiResult<AddMultipleSchedulesToOfferingResponse>>
{
    private readonly IMediator _mediator;

    public AddMultipleSchedulesToOfferingEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<AddMultipleSchedulesToOfferingResponse>> ExecuteAsync(
        AddMultipleSchedulesToOfferingRequest request, CancellationToken cancellationToken)
    {
        var schedules = request.Schedules.Select(s =>
            new AddMultipleSchedulesToOffering.ScheduleToAdd(
                s.DayOfWeek,
                s.StartTime,
                s.EndTime)).ToList();

        var result = await _mediator.Send(
            new AddMultipleSchedulesToOffering.Command(
                ClassSectionSubjectOfferingId.From(request.Id),
                schedules),
            cancellationToken);

        return result.ToCreatedResult(
            response => $"/subject-offerings/{request.Id}/schedules",
            response => new AddMultipleSchedulesToOfferingResponse
            {
                ScheduleIds = response.AddedScheduleIds.Select(id => id.Value).ToList()
            });
    }
}
