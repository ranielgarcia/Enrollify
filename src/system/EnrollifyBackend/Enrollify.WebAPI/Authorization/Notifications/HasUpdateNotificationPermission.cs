using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Notifications;

public class HasUpdateNotificationPermission : IAuthorizationRequirement
{
}

public class HasUpdateNotificationPermissionHandler : UserAuthorizationHandler<HasUpdateNotificationPermission>
{
    public HasUpdateNotificationPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateNotificationPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Notifications, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

