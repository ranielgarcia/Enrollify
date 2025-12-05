using Ardalis.SmartEnum.Dapper;
using Dapper;
using Enrollify.Core.Constants;
using Enrollify.Infrastructure.Data;
using Enrollify.Infrastructure.Data.Dapper.Generated;
using Enrollify.SharedKernel;

namespace Enrollify.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      ConfigurationManager config,
      ILogger logger)
    {
        // Try to get connection strings in order of priority:
        // 1. "cleanarchitecture" - provided by Aspire when using .WithReference(cleanArchDb)
        // 2. "DefaultConnection" - traditional SQL Server connection
        // 3. "SqliteConnection" - fallback to SQLite
        string? connectionString = config.GetConnectionString("cleanarchitecture")
                                   ?? config.GetConnectionString("DefaultConnection")
                                   ?? config.GetConnectionString("SqliteConnection");
        Guard.Against.Null(connectionString);

        services.AddTransient<IDbConnectionFactory>(sp =>
            new SqlConnectionFactory(connectionString));


        // Auto register all Vogen Dapper type handlers/converters
        VogenDapperTypeHandlerRegistration.RegisterTypeHandlers();
        SqlMapper.AddTypeHandler(typeof(PermissionScopeEnum), new SmartEnumByValueTypeHandler<PermissionScopeEnum>());


        services.AddScoped<EventDispatchInterceptor>();
        services.AddScoped<PreSaveChangesInterceptor>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        services.AddDbContext<EnrollifyDbContext>((provider, options) =>
        {
            var eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();
            var preSaveChangesInterceptor = provider.GetRequiredService<PreSaveChangesInterceptor>();
            
            options.UseSqlServer(connectionString);

            options.AddInterceptors(eventDispatchInterceptor);
            options.AddInterceptors(preSaveChangesInterceptor);
        });


        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
               .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        logger.LogInformation("{Project} services registered", "Infrastructure");

        return services;
    }
}
