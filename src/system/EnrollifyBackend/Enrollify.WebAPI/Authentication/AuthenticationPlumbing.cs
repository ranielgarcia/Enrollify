using Enrollify.Core.Services.Authentication;
using Enrollify.Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Enrollify.WebAPI.Authentication;

public static class AuthenticationPlumbing
{
    public static IServiceCollection AddAzureADAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<ICurrentUserAccessor, HttpUserAccessor>();

        return services;
    }

    public static IApplicationBuilder UseAzureADAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        // Order matters here as this caches the user context used by the auth handlers    
        app.UseMiddleware<UserContextMiddleware>();
        app.UseAuthorization();
        return app;
    }
}
