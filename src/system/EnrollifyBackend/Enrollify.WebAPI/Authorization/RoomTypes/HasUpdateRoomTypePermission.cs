using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public class HasUpdateRoomTypePermission : IAuthorizationRequirement
{
}

public class HasUpdateRoomTypePermissionHandler : UserAuthorizationHandler<HasUpdateRoomTypePermission>
{
    public HasUpdateRoomTypePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateRoomTypePermission requirement)
    {
        // Check if the user has the 'Update' permission for the 'RoomTypes' scope
        if (user.HasPermissionToTheScope(PermissionScopeEnum.RoomTypes, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
