using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectEquivalenceGroups;

public class HasViewSubjectEquivalenceGroupsPermission : IAuthorizationRequirement
{
}

public class HasViewSubjectEquivalenceGroupsPermissionHandler : UserAuthorizationHandler<HasViewSubjectEquivalenceGroupsPermission>
{
    public HasViewSubjectEquivalenceGroupsPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewSubjectEquivalenceGroupsPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectEquivalenceGroups, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
