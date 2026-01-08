using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Curriculums;

public class HasViewCurriculumsPermission : IAuthorizationRequirement
{
}

public class HasViewCurriculumnsPermissionHandler : UserAuthorizationHandler<HasViewCurriculumsPermission>
{
    public HasViewCurriculumnsPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewCurriculumsPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Curriculums, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}