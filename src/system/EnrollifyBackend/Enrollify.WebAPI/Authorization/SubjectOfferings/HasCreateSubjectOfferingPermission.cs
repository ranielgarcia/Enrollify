using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectOfferings;

public class HasCreateSubjectOfferingPermission : IAuthorizationRequirement;

public class HasCreateSubjectOfferingPermissionHandler : UserAuthorizationHandler<HasCreateSubjectOfferingPermission>
{
    public HasCreateSubjectOfferingPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateSubjectOfferingPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectOfferings, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
