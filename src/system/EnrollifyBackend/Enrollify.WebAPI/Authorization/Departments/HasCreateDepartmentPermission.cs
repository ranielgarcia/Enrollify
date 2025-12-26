using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Departments;

public class HasCreateDepartmentPermission : IAuthorizationRequirement
{
}

public class HasCreateDepartmentPermissionHandler : UserAuthorizationHandler<HasCreateDepartmentPermission>
{
    public HasCreateDepartmentPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateDepartmentPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Departments, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
