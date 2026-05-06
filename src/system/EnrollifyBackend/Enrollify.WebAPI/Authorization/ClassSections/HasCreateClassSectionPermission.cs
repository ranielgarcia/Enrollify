using System;
using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.ClassSections;

public class HasCreateClassSectionPermission : IAuthorizationRequirement;

public class HasCreateClassSectionPermissionHandler : UserAuthorizationHandler<HasCreateClassSectionPermission>
{
    public HasCreateClassSectionPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateClassSectionPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.ClassSections, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
