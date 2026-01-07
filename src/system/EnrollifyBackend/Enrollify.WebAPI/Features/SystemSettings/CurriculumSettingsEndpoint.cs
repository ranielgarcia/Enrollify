using Enrollify.Core.Models;
using Microsoft.Extensions.Options;

namespace Enrollify.WebAPI.Features.SystemSettings;

[HttpGet("curriculum-settings")]
[Group<SystemSettingsGroup>]
[Authorize(Policy = PolicyName.HasAnyValidRoleAndPermission)]
public class CurriculumSettingsEndpoint(IOptions<CurriculaSettings> options) : EndpointWithoutRequest<CurriculaSettings>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        await Send.OkAsync(options.Value);
    }
}
