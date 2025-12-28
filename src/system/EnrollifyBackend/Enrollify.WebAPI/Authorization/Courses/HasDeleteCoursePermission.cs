using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Courses;

public class HasDeleteCoursePermission : IAuthorizationRequirement
{
}

public class HasDeleteCoursePermissionHandler : UserAuthorizationHandler<HasDeleteCoursePermission>
{
    public HasDeleteCoursePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteCoursePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Courses, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}