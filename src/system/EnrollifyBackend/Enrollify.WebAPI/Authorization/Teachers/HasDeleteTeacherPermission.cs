using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Teachers;

public class HasDeleteTeacherPermission : IAuthorizationRequirement
{
}

public class HasDeleteTeacherPermissionHandler : UserAuthorizationHandler<HasDeleteTeacherPermission>
{
    public HasDeleteTeacherPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteTeacherPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Teachers, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
