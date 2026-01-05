using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Subjects;

public class HasCreateSubjectPermission : IAuthorizationRequirement
{
}

public class HasCreateSubjectPermissionHandler : UserAuthorizationHandler<HasCreateSubjectPermission>
{
    public HasCreateSubjectPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateSubjectPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Subjects, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}