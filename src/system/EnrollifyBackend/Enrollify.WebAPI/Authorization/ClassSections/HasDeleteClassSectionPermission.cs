using System;
using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.ClassSections;

public class HasDeleteClassSectionPermission : IAuthorizationRequirement;

public class HasDeleteClassSectionPermissionHandler : UserAuthorizationHandler<HasDeleteClassSectionPermission>
{
    public HasDeleteClassSectionPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteClassSectionPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.ClassSections, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
