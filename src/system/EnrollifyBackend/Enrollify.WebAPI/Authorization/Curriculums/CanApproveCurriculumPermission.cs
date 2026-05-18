using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization.Curriculums;

public class CanApproveCurriculumPermission : IAuthorizationRequirement
{
}

public class CanApproveCurriculumPermissionHandler : UserAuthorizationHandler<CanApproveCurriculumPermission>
{
    public CanApproveCurriculumPermissionHandler(IMediator mediator) : base(mediator) { }

    private static readonly List<RolesEnum> CanApproveCurriculumRoles = new List<RolesEnum>
    {
        RolesEnum.AcademicAdvisor
    };

    protected override Task CheckRequirement(UserContext user, AuthorizationHandlerContext context, CanApproveCurriculumPermission requirement)
    {
        if (user.HasPermissionToTheScope(PermissionScopeEnum.Curriculums, PermissionEnum.Update))
        {
            context.Succeed(requirement);
        }

        var validRoleNames = CanApproveCurriculumRoles.Select(r => r.Name).ToList();

        if (user.HasAnyValidRole() && user.GetValidRoles().Any(r => validRoleNames.Contains(r.Name.Value)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
