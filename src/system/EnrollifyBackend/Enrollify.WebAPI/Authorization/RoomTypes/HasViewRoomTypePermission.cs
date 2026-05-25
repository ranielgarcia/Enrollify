using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public class HasViewRoomTypePermission : IAuthorizationRequirement
{
}

public class HasViewRoomTypesPermissionHandler : UserAuthorizationHandler<HasViewRoomTypePermission>
{
    public HasViewRoomTypesPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewRoomTypePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.RoomTypes, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
