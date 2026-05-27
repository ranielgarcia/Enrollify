using Enrollify.Infrastructure;

namespace Enrollify.WebAPI.Plumbing;

public static class ServiceConfigs
{
    public static IServiceCollection AddServiceConfigs(
        this IServiceCollection services,
        Microsoft.Extensions.Logging.ILogger logger,
        WebApplicationBuilder builder)
    {
        services.AddInfrastructureServices(builder.Configuration, logger, builder.Environment.IsDevelopment())
            .AddMediatR(logger);

        return services;
    }
}
