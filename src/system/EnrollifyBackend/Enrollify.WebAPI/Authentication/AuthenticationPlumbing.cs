using Enrollify.Core.Authentication;
using Enrollify.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Enrollify.WebAPI.Authentication;

public static class AuthenticationPlumbing
{
    public static IServiceCollection AddAzureADAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        // SignalR's WebSocket/SSE transports can't set an Authorization header on the
        // connection handshake, so the client sends the token as an "access_token" query
        // string parameter instead. Chain onto whatever OnMessageReceived Microsoft.Identity.Web
        // already configured (registered after AddMicrosoftIdentityWebApi so it runs after it).
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var originalOnMessageReceived = options.Events?.OnMessageReceived;
            options.Events ??= new JwtBearerEvents();
            options.Events.OnMessageReceived = async context =>
            {
                if (originalOnMessageReceived is not null)
                    await originalOnMessageReceived(context);

                if (string.IsNullOrEmpty(context.Token) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(accessToken))
                        context.Token = accessToken;
                }
            };
        });

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
