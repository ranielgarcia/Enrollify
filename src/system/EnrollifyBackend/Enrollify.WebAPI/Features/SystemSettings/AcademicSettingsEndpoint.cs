using Enrollify.Core;

namespace Enrollify.WebAPI.Features.SystemSettings;

[HttpGet("academic-settings")]
[Group<SystemSettingsGroup>]
[Authorize(Policy = PolicyName.HasAnyValidRoleAndPermission)]
public class AcademicSettingsEndpoint() : EndpointWithoutRequest<Core.AcademicSettings>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        await Send.OkAsync(new AcademicSettings());
    }
}
