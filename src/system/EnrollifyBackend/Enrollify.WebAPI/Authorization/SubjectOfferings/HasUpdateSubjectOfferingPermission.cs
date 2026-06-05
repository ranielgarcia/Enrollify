using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectOfferings;

public class HasUpdateSubjectOfferingPermission : IAuthorizationRequirement;

public class HasUpdateSubjectOfferingPermissionHandler : UserAuthorizationHandler<HasUpdateSubjectOfferingPermission>
{
    public HasUpdateSubjectOfferingPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateSubjectOfferingPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectOfferings, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
