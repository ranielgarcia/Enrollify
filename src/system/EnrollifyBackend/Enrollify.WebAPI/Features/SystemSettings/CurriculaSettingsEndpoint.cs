using Enrollify.Core.Models;
using Microsoft.Extensions.Options;

namespace Enrollify.WebAPI.Features.SystemSettings;

[HttpGet("curricula-settings")]
[Group<SystemSettingsGroup>]
[AllowAnonymous]
//[Authorize(Policy = PolicyName.HasAnyValidRoleAndPermission)]
public class CurriculaSettingsEndpoint(IOptions<CurriculaSettings> options) : EndpointWithoutRequest<CurriculaSettings>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        await Send.OkAsync(options.Value);
    }
}
