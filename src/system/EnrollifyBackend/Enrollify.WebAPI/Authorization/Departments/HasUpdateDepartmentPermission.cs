using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Departments;

public class HasUpdateDepartmentPermission : IAuthorizationRequirement
{
}

public class HasUpdateDepartmentPermissionHandler : UserAuthorizationHandler<HasUpdateDepartmentPermission>
{
    public HasUpdateDepartmentPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateDepartmentPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Departments, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}