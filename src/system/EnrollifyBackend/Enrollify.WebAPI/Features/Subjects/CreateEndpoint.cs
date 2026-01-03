using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.WebAPI.Features.Subjects;

public class CreateSubjectResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Units { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PreferRoomTypeId { get; set; }
}

public class CreateSubjectRequest
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Units { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PreferRoomTypeId { get; set; }
}

public class CreateSubjectRequestValidator : Validator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
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

[HttpPost("")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateSubjectPermission)]
public class CreateEndpoint : Endpoint<CreateSubjectRequest, Results<Created<CreateSubjectResponse>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    private readonly IMediator _mediator;
    public CreateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override async Task<Results<Created<CreateSubjectResponse>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync (CreateSubjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Application.Subjects.Features.CreateSubject.Command(
            new SubjectForCreation
            {
                Code = SubjectCode.From(request.Code),
                Title = request.Title,
                Units = request.Units,
                Description = request.Description,
                PreferRoomTypeId = RoomTypeId.From(request.PreferRoomTypeId)
            }), cancellationToken);
        
        return result.ToCreatedResult(
            id => $"/subjects/{id}",
            id => new CreateSubjectResponse
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