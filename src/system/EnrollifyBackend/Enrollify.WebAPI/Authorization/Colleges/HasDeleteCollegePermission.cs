using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Colleges;

public class HasDeleteCollegePermission : IAuthorizationRequirement
{
}

public class HasDeleteCollegePermissionHandler : UserAuthorizationHandler<HasDeleteCollegePermission>
{
    public HasDeleteCollegePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteCollegePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Colleges, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
