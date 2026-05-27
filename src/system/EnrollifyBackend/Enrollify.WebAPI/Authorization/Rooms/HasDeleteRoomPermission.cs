using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Rooms;

public class HasDeleteRoomPermission : IAuthorizationRequirement
{
}

public class HasDeleteRoomPermissionHandler : UserAuthorizationHandler<HasDeleteRoomPermission>
{
    public HasDeleteRoomPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteRoomPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Rooms, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
