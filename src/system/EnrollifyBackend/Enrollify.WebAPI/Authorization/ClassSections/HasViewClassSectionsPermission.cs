using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.ClassSections;

public class HasViewClassSectionsPermission : IAuthorizationRequirement;

public class HasViewClassSectionPermissionHandler : UserAuthorizationHandler<HasViewClassSectionsPermission>
{
    public HasViewClassSectionPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewClassSectionsPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.ClassSections, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
