using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Curriculums;

public class HasUpdateCurriculumPermission : IAuthorizationRequirement
{
}

public class HasUpdateCurriculumPermissionHandler : UserAuthorizationHandler<HasUpdateCurriculumPermission>
{
    public HasUpdateCurriculumPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateCurriculumPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Curriculums, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
