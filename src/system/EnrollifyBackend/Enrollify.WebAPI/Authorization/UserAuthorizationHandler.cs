using Enrollify.Application.Authentication.GetContext;
using Enrollify.Core.Authentication;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization;

public abstract class UserAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement>
    where TRequirement : IAuthorizationRequirement
{
    private readonly IMediator _mediator;

    protected UserAuthorizationHandler(IMediator mediator)
    {
        _mediator = mediator;
        if (_mediator is null)
        {
            throw new ArgumentNullException("_mediator", $"{nameof(IMediator)} is required");
        }
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        var currentUser = await _mediator.Send(new GetCurrentUserContextQuery());
        if (currentUser is null || !currentUser.IsSuccess)
        {
            context.Fail(new AuthorizationFailureReason(this, "Cannot process authorization requirement without an authenticated user"));
            return;
        }

        if (!currentUser.Value.HasAnyValidRole())
        {
            context.Fail(new AuthorizationFailureReason(this, "Cannot process authorization requirement on a user without a role or any valid role"));
            return;
        }

        if (!currentUser.Value.Roles.Any(r => r.PermissionScopes.Count > 0))
        {
            context.Fail(new AuthorizationFailureReason(this, "Cannot process authorization requirement on a user without any permissions"));
            return;
        }

        await CheckRequirement(currentUser, context, requirement);
    }

    protected abstract Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, TRequirement requirement);
}
