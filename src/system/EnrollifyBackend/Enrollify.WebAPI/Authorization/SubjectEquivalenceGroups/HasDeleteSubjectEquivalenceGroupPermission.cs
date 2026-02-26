using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.SubjectEquivalenceGroups;

public class HasDeleteSubjectEquivalenceGroupPermission : IAuthorizationRequirement
{
}

public class HasDeleteSubjectEquivalenceGroupPermissionHandler : UserAuthorizationHandler<HasDeleteSubjectEquivalenceGroupPermission>
{
    public HasDeleteSubjectEquivalenceGroupPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteSubjectEquivalenceGroupPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.SubjectEquivalenceGroups, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
