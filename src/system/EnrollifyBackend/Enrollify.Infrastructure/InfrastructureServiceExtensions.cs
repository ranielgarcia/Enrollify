using Ardalis.SmartEnum;
using Ardalis.SmartEnum.Dapper;
using Dapper;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Infrastructure.Data;
using Enrollify.Infrastructure.Data.Dapper.Generated;
using Enrollify.SharedKernel;
using System.Reflection;

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
        
        // Auto register all SmartEnum Dapper type handlers
        RegisterSmartEnumTypeHandlers(typeof(PermissionScopeEnum).Assembly);


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

    /// <summary>
    /// Registers Dapper type handlers for all SmartEnum types found in the specified assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for SmartEnum types. If none provided, scans the calling assembly.</param>
    public static void RegisterSmartEnumTypeHandlers(params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var smartEnumTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } 
                          && IsSmartEnum(type));

        foreach (var smartEnumType in smartEnumTypes)
        {
            var handlerType = typeof(SmartEnumByValueTypeHandler<>).MakeGenericType(smartEnumType);
            var handler = Activator.CreateInstance(handlerType);
            SqlMapper.AddTypeHandler(smartEnumType, (SqlMapper.ITypeHandler)handler!);
        }
    }

    private static bool IsSmartEnum(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType)
            {
                var genericTypeDef = baseType.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(SmartEnum<>) || 
                    genericTypeDef == typeof(SmartEnum<,>))
                {
                    return true;
                }
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
