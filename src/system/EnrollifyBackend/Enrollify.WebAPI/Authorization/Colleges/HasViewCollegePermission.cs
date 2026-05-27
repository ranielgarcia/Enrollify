using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Colleges;

public class HasViewCollegePermission : IAuthorizationRequirement
{
}

public class HasViewCollegePermissionHandler : UserAuthorizationHandler<HasViewCollegePermission>
{
    public HasViewCollegePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewCollegePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Colleges, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
