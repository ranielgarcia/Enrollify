using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Notifications;

public class HasViewNotificationPermission : IAuthorizationRequirement
{
}

public class HasViewNotificationPermissionHandler : UserAuthorizationHandler<HasViewNotificationPermission>
{
    public HasViewNotificationPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewNotificationPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Notifications, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
