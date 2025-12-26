using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Departments;

public class HasViewDepartmentPermission : IAuthorizationRequirement
{
}

public class HasViewDepartmentPermissionHandler : UserAuthorizationHandler<HasViewDepartmentPermission>
{
    public HasViewDepartmentPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewDepartmentPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Departments, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}