using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectEquivalenceGroups;

public class HasCreateSubjectEquivalenceGroupPermission : IAuthorizationRequirement
{
}

public class HasCreateSubjectEquivalenceGroupPermissionHandler : UserAuthorizationHandler<HasCreateSubjectEquivalenceGroupPermission>
{
    public HasCreateSubjectEquivalenceGroupPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateSubjectEquivalenceGroupPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectEquivalenceGroups, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
