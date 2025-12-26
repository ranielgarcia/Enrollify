using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Departments;

public class HasDeleteDepartmentPermission : IAuthorizationRequirement
{
}

public class HasDeleteDepartmentPermissionHandler : UserAuthorizationHandler<HasDeleteDepartmentPermission>
{
    public HasDeleteDepartmentPermissionHandler(IMediator mediator) : base(mediator) { }

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteDepartmentPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Departments, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
