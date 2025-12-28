using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Courses;

public class HasUpdateCoursePermission : IAuthorizationRequirement
{
}

public class HasUpdateCoursePermissionHandler : UserAuthorizationHandler<HasUpdateCoursePermission>
{
    public HasUpdateCoursePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateCoursePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Courses, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
