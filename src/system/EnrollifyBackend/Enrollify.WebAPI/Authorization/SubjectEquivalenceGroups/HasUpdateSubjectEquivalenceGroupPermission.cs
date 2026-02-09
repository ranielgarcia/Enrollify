using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectEquivalenceGroups;

public class HasUpdateSubjectEquivalenceGroupPermission : IAuthorizationRequirement
{
}

public class HasUpdateSubjectEquivalenceGroupPermissionHandler : UserAuthorizationHandler<HasUpdateSubjectEquivalenceGroupPermission>
{
    public HasUpdateSubjectEquivalenceGroupPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateSubjectEquivalenceGroupPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectEquivalenceGroups, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
