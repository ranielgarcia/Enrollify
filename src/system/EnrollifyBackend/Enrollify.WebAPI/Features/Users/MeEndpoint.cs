using Enrollify.Application.Authentication.GetContext;
using Enrollify.Core.Authentication;

namespace Enrollify.WebAPI.Features.Users;

[HttpGet("me")]
[Authorize(Policy = PolicyName.HasAnyValidRoleAndPermission)]
public class MeEndpoint : EndpointWithoutRequest<OkOrNotFoundApiResult<UserContext>>
{
    private readonly IMediator _mediator;

    public MeEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OkOrNotFoundApiResult<UserContext>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserContextQuery(), ct);
        return result.ToGetByIdResult((UserContext userContext) => userContext);
    }

}
