using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Courses;

public class HasCreateCoursePermission : IAuthorizationRequirement
{
}

public class HasCreateCoursePermissionHandler : UserAuthorizationHandler<HasCreateCoursePermission>
{
    public HasCreateCoursePermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateCoursePermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Courses, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
