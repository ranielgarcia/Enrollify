using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.AcademicYearsAndTerms;

public class HasDeleteAcademicYearAndTermPermission : IAuthorizationRequirement
{
}

public class HasDeleteAcademicYearAndTermPermissionHandler : UserAuthorizationHandler<HasDeleteAcademicYearAndTermPermission>
{
    public HasDeleteAcademicYearAndTermPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasDeleteAcademicYearAndTermPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.AcademicYearsAndTerms, PermissionEnum.Delete))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
