using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Curriculums;

public class HasDeleteCurriculumPermission : IAuthorizationRequirement
{
}

public class HasDeleteCurriculumPermissionHandler : UserAuthorizationHandler<HasDeleteCurriculumPermission>
{
    public HasDeleteCurriculumPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteCurriculumPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Curriculums, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}