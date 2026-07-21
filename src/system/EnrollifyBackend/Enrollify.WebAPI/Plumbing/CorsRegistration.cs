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
                        .AllowAnyHeader();
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
