using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Buildings;

public class HasViewBuildingPermission : IAuthorizationRequirement
{
}

public class HasViewBuildingPermissionHandler : UserAuthorizationHandler<HasViewBuildingPermission>
{
    public HasViewBuildingPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewBuildingPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Buildings, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
