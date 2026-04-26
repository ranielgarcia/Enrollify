using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Teachers;

public class HasUpdateTeacherPermission : IAuthorizationRequirement
{
}

public class HasUpdateTeacherPermissionHandler : UserAuthorizationHandler<HasUpdateTeacherPermission>
{
    public HasUpdateTeacherPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateTeacherPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Teachers, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
