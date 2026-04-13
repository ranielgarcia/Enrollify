using Ardalis.SmartEnum;
using Ardalis.SmartEnum.Dapper;
using Azure.Storage.Blobs;
using Dapper;
using Enrollify.Application.Buildings;
using Enrollify.Application.Colleges;
using Enrollify.Application.Courses;
using Enrollify.Application.Curriculums;
using Enrollify.Application.Departments;
using Enrollify.Application.Roles.Features.List;
using Enrollify.Application.Rooms;
using Enrollify.Application.RoomTypes;
using Enrollify.Application.SubjectEquivalences;
using Enrollify.Application.Subjects;
using Enrollify.Application.Teachers;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.FileStorage;
using Enrollify.Infrastructure.Data;
using Enrollify.Infrastructure.Data.Dapper.Generated;
using Enrollify.Infrastructure.Data.Queries;
using Enrollify.Infrastructure.FileStorage;
using Enrollify.Infrastructure.Repositories;
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
        
        // Auto register all SmartEnum Dapper type handlers
        RegisterSmartEnumTypeHandlers(typeof(PermissionScopeEnum).Assembly);
        SqlMapper.AddTypeHandler(new SmartFlagEnumDapperTypeHandler<PermissionEnum>());

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

        services.AddScoped<IListRolesQueryService, ListRolesQueryService>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<ICollegeRepository, CollegeRepository>();
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ICurriculumRepository, CurriculumRepository>();
        services.AddScoped<ISubjectEquivalenceGroupRepository, SubjectEquivalenceGroupRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();

        // Azure Blob Storage
        string? azureBlobStorageConnectionString = config.GetConnectionString("AzureBlobStorage");
        if (!string.IsNullOrEmpty(azureBlobStorageConnectionString))
        {
            services.AddSingleton(new BlobServiceClient(azureBlobStorageConnectionString));
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            logger.LogInformation("{Service} registered with Azure Blob Storage", nameof(IFileStorageService));
        }
        else
        {
            logger.LogError("Azure Blob Storage connection string not found. {Service} will not be available. Add 'AzureBlobStorage' to ConnectionStrings configuration.", nameof(IFileStorageService));
        }

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
