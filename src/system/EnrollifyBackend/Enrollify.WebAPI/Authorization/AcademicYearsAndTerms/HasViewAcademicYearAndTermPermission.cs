using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.AcademicYearsAndTerms;

public class HasViewAcademicYearAndTermPermission : IAuthorizationRequirement
{
}

public class HasViewAcademicYearAndTermPermissionHandler : UserAuthorizationHandler<HasViewAcademicYearAndTermPermission>
{
    public HasViewAcademicYearAndTermPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasViewAcademicYearAndTermPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.AcademicYearsAndTerms, PermissionEnum.View))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
