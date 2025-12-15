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
        // Check if the user has the 'Update' permission for the 'RoomTypes' scope
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Rooms, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}