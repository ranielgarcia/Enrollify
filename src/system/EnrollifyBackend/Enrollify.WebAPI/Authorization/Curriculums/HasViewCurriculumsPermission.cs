using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Curriculums;

public class HasViewCurriculumsPermission : IAuthorizationRequirement
{
}

public class HasViewCurriculumsPermissionHandler : UserAuthorizationHandler<HasViewCurriculumsPermission>
{
    public HasViewCurriculumsPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewCurriculumsPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Curriculums, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}