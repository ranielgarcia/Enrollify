using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Rooms;

public class HasUpdateRoomPermission : IAuthorizationRequirement
{
}

public class HasUpdateRoomPermissionHandler : UserAuthorizationHandler<HasUpdateRoomPermission>
{
    public HasUpdateRoomPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateRoomPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Rooms, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}