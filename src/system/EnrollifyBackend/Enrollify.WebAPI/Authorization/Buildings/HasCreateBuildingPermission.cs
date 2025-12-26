using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Buildings;

public class HasCreateBuildingPermission : IAuthorizationRequirement
{
}

public class HasCreateBuildingPermissionHandler : UserAuthorizationHandler<HasCreateBuildingPermission>
{
    public HasCreateBuildingPermissionHandler(IMediator mediator) : base(mediator) { }

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateBuildingPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Buildings, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}