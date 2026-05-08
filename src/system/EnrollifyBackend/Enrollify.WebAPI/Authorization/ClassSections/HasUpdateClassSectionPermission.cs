using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.ClassSections;

public class HasUpdateClassSectionPermission : IAuthorizationRequirement;

public class HasUpdateClassSectionPermissionHandler : UserAuthorizationHandler<HasUpdateClassSectionPermission>
{
    public HasUpdateClassSectionPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateClassSectionPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.ClassSections, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
