using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Rooms;

public class HasViewRoomsPermission : IAuthorizationRequirement
{
}

public class HasViewRoomsPermissionHandler : UserAuthorizationHandler<HasViewRoomsPermission>
{
    public HasViewRoomsPermissionHandler(IMediator mediator) : base(mediator) { }

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewRoomsPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Rooms, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}