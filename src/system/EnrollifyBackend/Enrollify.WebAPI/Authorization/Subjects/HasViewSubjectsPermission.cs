using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Subjects;

public class HasViewSubjectsPermission : IAuthorizationRequirement
{
}

public class HasViewSubjectsPermissionHandler : UserAuthorizationHandler<HasViewSubjectsPermission>
{
    public HasViewSubjectsPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewSubjectsPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Subjects, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}