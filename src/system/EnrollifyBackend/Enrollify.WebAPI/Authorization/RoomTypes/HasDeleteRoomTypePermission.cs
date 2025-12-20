using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public class HasDeleteRoomTypePermission : IAuthorizationRequirement
{
}

public class HasDeleteRoomTypePermissionHandler : UserAuthorizationHandler<HasDeleteRoomTypePermission>
{
    public HasDeleteRoomTypePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteRoomTypePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.RoomTypes, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
