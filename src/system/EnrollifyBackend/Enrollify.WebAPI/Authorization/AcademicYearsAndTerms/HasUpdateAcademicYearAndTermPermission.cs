using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.AcademicYearsAndTerms;

public class HasUpdateAcademicYearAndTermPermission : IAuthorizationRequirement
{
}

public class HasUpdateAcademicYearAndTermPermissionHandler : UserAuthorizationHandler<HasUpdateAcademicYearAndTermPermission>
{
    public HasUpdateAcademicYearAndTermPermissionHandler(IMediator mediator) : base(mediator) { }
    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, HasUpdateAcademicYearAndTermPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.AcademicYearsAndTerms, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
