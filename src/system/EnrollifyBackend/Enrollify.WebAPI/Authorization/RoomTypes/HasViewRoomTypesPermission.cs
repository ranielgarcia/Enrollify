using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public class HasViewRoomTypesPermission : IAuthorizationRequirement
{
}

public class HasViewRoomTypesPermissionHandler : UserAuthorizationHandler<HasViewRoomTypesPermission>
{
    public HasViewRoomTypesPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewRoomTypesPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.RoomTypes, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}