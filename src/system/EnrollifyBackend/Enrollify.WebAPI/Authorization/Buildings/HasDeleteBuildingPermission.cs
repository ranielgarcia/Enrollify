using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Buildings;

public class HasDeleteBuildingPermission : IAuthorizationRequirement
{
}

public class HasDeleteBuildingPermissionHandler : UserAuthorizationHandler<HasDeleteBuildingPermission>
{
    public HasDeleteBuildingPermissionHandler(IMediator mediator) : base(mediator) { }

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteBuildingPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Buildings, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}