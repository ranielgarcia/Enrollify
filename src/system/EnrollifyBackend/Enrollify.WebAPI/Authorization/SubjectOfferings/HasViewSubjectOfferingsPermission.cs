using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectOfferings;

public class HasViewSubjectOfferingsPermission : IAuthorizationRequirement;

public class HasViewSubjectOfferingsPermissionHandler : UserAuthorizationHandler<HasViewSubjectOfferingsPermission>
{
    public HasViewSubjectOfferingsPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewSubjectOfferingsPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectOfferings, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
