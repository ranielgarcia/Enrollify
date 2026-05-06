using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.AcademicYearsAndTerms;

public class HasCreateAcademicYearAndTermPermission : IAuthorizationRequirement
{
}

public class HasCreateAcademicYearAndTermPermissionHandler : UserAuthorizationHandler<HasCreateAcademicYearAndTermPermission>
{
    public HasCreateAcademicYearAndTermPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasCreateAcademicYearAndTermPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.AcademicYearsAndTerms, PermissionEnum.Create))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
