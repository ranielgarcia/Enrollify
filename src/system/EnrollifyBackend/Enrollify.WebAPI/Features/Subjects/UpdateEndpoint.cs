using Enrollify.Application.Features.Subjects.Commands;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.WebAPI.Features.Subjects;

public class UpdateSubjectResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Units { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PreferRoomTypeId { get; set; }
}

public class UpdateSubjectRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Units { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PreferRoomTypeId { get; set; }
}

public class UpdateSubjectRequestValidator : Validator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid subject ID.");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Please provide a subject code.")
            .MaximumLength(20).WithMessage("Code must be 20 characters or fewer.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Please provide a subject title.")
            .MaximumLength(100).WithMessage("Title must be 100 characters or fewer.");
        RuleFor(x => x.Units)
            .GreaterThan(0).WithMessage("Units must be greater than zero.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please provide a subject description.")
            .MaximumLength(255).WithMessage("Description must be 255 characters or fewer.");
        RuleFor(x => x.PreferRoomTypeId)
            .NotNull().WithMessage("Please provide a valid room type ID.");
    }
}

[HttpPut("{id:int}")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectPermission)]
public class UpdateEndpoint : Endpoint<UpdateSubjectRequest, OkOrNotFoundApiResult<UpdateSubjectResponse>>
{
    private readonly IMediator _mediator;
    public UpdateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override async Task<OkOrNotFoundApiResult<UpdateSubjectResponse>>
        ExecuteAsync(UpdateSubjectRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateSubject.Command
        {
            Id = SubjectId.From(request.Id),
            Code = SubjectCode.From(request.Code),
            Title = request.Title,
            Units = request.Units,
            Description = request.Description,
            PreferRoomTypeId = RoomTypeId.From(request.PreferRoomTypeId)
        }, ct);

        return result.ToUpdateResult(
            id => new UpdateSubjectResponse
            {
                Id = id.Value,
                Code = request.Code,
                Title = request.Title,
                Units = request.Units,
                Description = request.Description,
                PreferRoomTypeId = request.PreferRoomTypeId
            });
    }
}
