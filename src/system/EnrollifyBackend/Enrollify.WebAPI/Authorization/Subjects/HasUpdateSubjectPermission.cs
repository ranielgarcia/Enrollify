using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Subjects;

public class HasUpdateSubjectPermission : IAuthorizationRequirement
{
}

public class HasUpdateSubjectPermissionHandler : UserAuthorizationHandler<HasUpdateSubjectPermission>
{
    public HasUpdateSubjectPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateSubjectPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Subjects, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
