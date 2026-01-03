using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Subjects;

public class HasDeleteSubjectPermission : IAuthorizationRequirement 
{
}

public class HasDeleteSubjectPermissionHandler : UserAuthorizationHandler<HasDeleteSubjectPermission>
{
    public HasDeleteSubjectPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteSubjectPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Subjects, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}