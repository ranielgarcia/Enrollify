namespace Enrollify.WebAPI.StartupServices;

public static class StartupServicesRegistration
{
    public static IServiceCollection AddStartupServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Register the StartupRunner as a hosted service
        services.AddHostedService<StartupRunner>();

        // Register individual startup services here
        services.AddScoped<IStartupService, FileStorageRegistration>();

        if (environment.IsDevelopment())
        {
            services.AddScoped<IStartupService, DevelopmentDatabaseSetupService>();
        }

        return services;
    }
}
