using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Roles.View;

public class HasViewRolesPermission : IAuthorizationRequirement
{
}

public class HasViewRolesPermissionHandler : UserAuthorizationHandler<HasViewRolesPermission>
{
    public HasViewRolesPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewRolesPermission requirement)
    {
        // Check if the user has the 'View' permission for the 'Roles' scope
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Roles, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}