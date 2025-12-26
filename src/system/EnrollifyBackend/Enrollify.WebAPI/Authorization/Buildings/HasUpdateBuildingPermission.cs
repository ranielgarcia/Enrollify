using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Buildings;

public class HasUpdateBuildingPermission : IAuthorizationRequirement
{
}

public class HasUpdateBuildingPermissionHandler : UserAuthorizationHandler<HasUpdateBuildingPermission>
{
    public HasUpdateBuildingPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateBuildingPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Buildings, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}