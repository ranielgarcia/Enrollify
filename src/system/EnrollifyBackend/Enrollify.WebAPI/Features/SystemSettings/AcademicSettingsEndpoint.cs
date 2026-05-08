using Enrollify.Core;

namespace Enrollify.WebAPI.Features.SystemSettings;

[HttpGet("academic-core-settings")]
[Group<SystemSettingsGroup>]
[Authorize(Policy = PolicyName.HasAnyValidRoleAndPermission)]
public class AcademicSettingsEndpoint() : EndpointWithoutRequest<Core.AcademicCoreSettings>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        await Send.OkAsync(new AcademicCoreSettings());
    }
}
