using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectOfferings;

public class HasDeleteSubjectOfferingPermission : IAuthorizationRequirement;

public class HasDeleteSubjectOfferingPermissionHandler : UserAuthorizationHandler<HasDeleteSubjectOfferingPermission>
{
    public HasDeleteSubjectOfferingPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteSubjectOfferingPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectOfferings, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
