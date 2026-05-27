using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Colleges;

public class HasUpdateCollegePermission : IAuthorizationRequirement
{
}

public class HasUpdateCollegePermissionHandler : UserAuthorizationHandler<HasUpdateCollegePermission>
{
    public HasUpdateCollegePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateCollegePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Colleges, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
