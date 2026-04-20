using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Teachers;

public class HasViewTeacherPermission : IAuthorizationRequirement
{
}

public class HasViewTeacherPermissionHandler : UserAuthorizationHandler<HasViewTeacherPermission>
{
    public HasViewTeacherPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewTeacherPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Teachers, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
