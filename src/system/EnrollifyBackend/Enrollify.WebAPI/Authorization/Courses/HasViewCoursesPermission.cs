using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Courses;

public class HasViewCoursesPermission : IAuthorizationRequirement   
{
}

public class HasViewCoursesPermissionHandler : UserAuthorizationHandler<HasViewCoursesPermission>
{
    public HasViewCoursesPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewCoursesPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Courses, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}