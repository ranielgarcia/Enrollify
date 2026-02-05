using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Curriculums;

public class HasCreateCurriculumPermission : IAuthorizationRequirement
{
}

public class HasCreateCurriculumPermissionHandler : UserAuthorizationHandler<HasCreateCurriculumPermission>
{
    public HasCreateCurriculumPermissionHandler(IMediator mediator) : base(mediator) { }

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateCurriculumPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Curriculums, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
