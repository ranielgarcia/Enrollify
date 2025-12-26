using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Rooms;

public class HasCreateRoomPermission : IAuthorizationRequirement
{
}

public class HasCreateRoomPermissionHandler : UserAuthorizationHandler<HasCreateRoomPermission>
{
    public HasCreateRoomPermissionHandler(IMediator mediator) : base(mediator) { }

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateRoomPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Rooms, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
