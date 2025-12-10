using Enrollify.Application.RoomTypes;
using Enrollify.WebAPI.Authorization;
using Enrollify.WebAPI.Extensions;
using FastEndpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Features.RoomTypes;

public class CreateRoomTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }
}

public class CreateRoomTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }
}

public class CreateRoomTypeRequestValidator : Validator<CreateRoomTypeRequest>
{
    public CreateRoomTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

[HttpPost("")]
[Group<RoomTypeEndpoingGroup>]
[Authorize(Policy = PolicyName.HasCreateRoomTypePermission)]
public class CreateEndpoint : Endpoint<CreateRoomTypeRequest, Results<Created<CreateRoomTypeResponse>, ValidationProblem, ProblemHttpResult>>
{
    private readonly IMediator _mediator;

    public CreateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Created<CreateRoomTypeResponse>, ValidationProblem, ProblemHttpResult>> 
        ExecuteAsync (CreateRoomTypeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateRoomType.Command(request.Name, request.Description));

        return result.ToCreatedResult(
            id => $"/room-types/{id}",
            id => new CreateRoomTypeResponse
            {
                Id = id.Value,
                Name = request.Name,
                Description = request.Description
            });
    }
}
