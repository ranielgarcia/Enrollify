using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Teachers;

public class HasCreateTeacherPermission : IAuthorizationRequirement
{
}

public class HasCreateTeacherPermissionHandler : UserAuthorizationHandler<HasCreateTeacherPermission>
{
    public HasCreateTeacherPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateTeacherPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Teachers, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
