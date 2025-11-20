using Microsoft.Net.Http.Headers;
using Serilog;

namespace Enrollify.WebAPI.Plumbing;

public static class CorsRegistration
{
    public static IServiceCollection AddGlobalCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedHosts = configuration.GetSection("CORS:AllowedHosts").Get<string[]>();

        if (allowedHosts is not null)
        {
            foreach (var host in allowedHosts)
            {
                Log.Information("CORS {AllowedHost}", host);
            }

            services.AddCors(o =>
            {
                o.AddDefaultPolicy(p =>
                {
                    p.AllowCredentials()
                        .AllowAnyMethod()
                        .WithOrigins(allowedHosts)
                        .WithHeaders(HeaderNames.Authorization,
                            HeaderNames.Accept,
                            HeaderNames.AcceptLanguage,
                            HeaderNames.ContentLanguage,
                            HeaderNames.ContentType,
                            HeaderNames.TraceParent);
                });
            });
        }

        return services;
    }

    public static IApplicationBuilder UseGlobalCorsPolicy(this IApplicationBuilder app)
    {
        app.UseCors();

        return app;
    }
}
