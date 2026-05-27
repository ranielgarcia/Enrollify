using Enrollify.Core.Authentication;
using Enrollify.WebAPI.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Shared;

public class HasAnyValidRoleHandler : UserAuthorizationHandler<HasAnyValidRole>
{
    public HasAnyValidRoleHandler(IMediator mediator) : base(mediator) { }

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasAnyValidRole requirement)
    {
        // The logic is already implemented in the parent abstract class
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
