using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Colleges;

public class HasCreateCollegePermission : IAuthorizationRequirement
{
}

public class HasCreateCollegePermissionHandler : UserAuthorizationHandler<HasCreateCollegePermission>
{
    public HasCreateCollegePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateCollegePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Colleges, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
