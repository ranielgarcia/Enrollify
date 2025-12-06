using Enrollify.Application.Authentication.GetContext;
using Enrollify.Core.Authentication;
using Enrollify.WebAPI.Authorization;
using Enrollify.WebAPI.Extensions;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Features.Users;

[HttpGet("me")]
[Authorize(Policy = PolicyName.HasAnyValidRoleAndPermission)]
public class MeEndpoint : EndpointWithoutRequest<Results<Ok<UserContext>, NotFound, ProblemHttpResult>>
{
    private readonly IMediator _mediator;

    public MeEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Ok<UserContext>, NotFound, ProblemHttpResult>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserContextQuery(), ct);
        return result.ToGetByIdResult((UserContext userContext) => userContext);
    }

}
