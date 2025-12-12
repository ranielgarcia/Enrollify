using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public class HasCreateRoomTypePermission : IAuthorizationRequirement
{
}

public class HasCreateRoomTypePermissionHandler : UserAuthorizationHandler<HasCreateRoomTypePermission>
{
    public HasCreateRoomTypePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateRoomTypePermission requirement)
    {
        // Check if the user has the 'View' permission for the 'Roles' scope
        if (user.HasPermissionToTheScope(PermissionScopeEnum.RoomTypes, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}