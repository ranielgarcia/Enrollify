using Ardalis.SmartEnum;
using Ardalis.SmartEnum.Dapper;
using Azure.Storage.Blobs;
using Dapper;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Application.Features.Buildings;
using Enrollify.Application.Features.ClassSections;
using Enrollify.Application.Features.ClassSectionSubjectOfferings;
using Enrollify.Application.Features.Colleges;
using Enrollify.Application.Features.CourseCurriculumAssignments;
using Enrollify.Application.Features.Courses;
using Enrollify.Application.Features.Curriculums;
using Enrollify.Application.Features.Departments;
using Enrollify.Application.Features.Roles.Queries;
using Enrollify.Application.Features.Rooms;
using Enrollify.Application.Features.RoomTypes;
using Enrollify.Application.Features.SubjectEquivalences;
using Enrollify.Application.Features.Subjects;
using Enrollify.Application.Features.Teachers;
using Enrollify.Application.Features.Teachers.Storage;
using Enrollify.Application.Features.ClassSchedules.Services;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services;
using Enrollify.Core.Services.ScheduleConflictDetection;
using Enrollify.Infrastructure.Data;
using Enrollify.Infrastructure.Data.Dapper.Generated;
using Enrollify.Infrastructure.Data.Queries;
using Enrollify.Infrastructure.Persistence;
using Enrollify.Infrastructure.Repositories;
using Enrollify.Infrastructure.Services;
using Enrollify.Infrastructure.Storage;
using Enrollify.SharedKernel;

namespace Enrollify.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      ConfigurationManager config,
      ILogger logger,
      bool isDevelopment = false)
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

        // Azure Blob Storage
        services.AddStorageSettings(config);

        // Auto register all Vogen Dapper type handlers/converters
        VogenDapperTypeHandlerRegistration.RegisterTypeHandlers();

        // Auto register all SmartEnum Dapper type handlers
        RegisterSmartEnumTypeHandlers(typeof(PermissionScopeEnum).Assembly);
        SqlMapper.AddTypeHandler(new SmartFlagEnumDapperTypeHandler<PermissionEnum>());

        // Register custom type handler for TimeSpan → TimeOnly conversion
        // This is required because Dapper doesn't have built-in support for converting
        // SQL Server TIME columns (TimeSpan) to .NET TimeOnly type
        SqlMapper.AddTypeHandler(new TimeSpanToTimeOnlyDapperTypeHandler());

        services.AddScoped<EventDispatchInterceptor>();
        services.AddScoped<PreSaveChangesInterceptor>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        services.AddDbContext<EnrollifyDbContext>((provider, options) =>
        {
            var eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();
            var preSaveChangesInterceptor = provider.GetRequiredService<PreSaveChangesInterceptor>();

            options.UseSqlServer(connectionString);

            if (isDevelopment)
                options.EnableSensitiveDataLogging();

            options.AddInterceptors(eventDispatchInterceptor);
            options.AddInterceptors(preSaveChangesInterceptor);
        });


        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
               .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddSingleton<IDbExceptionTranslator, SqlServerExceptionTranslator>();
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
        services.AddScoped<ITeacherPhotoStorageService, TeacherPhotoStorageService>();
        services.AddScoped<IAcademicYearAndTermRepository, AcademicYearAndTermRepository>();
        services.AddScoped<IClassSectionRepository, ClassSectionRepository>();
        services.AddScoped<IClassSectionSubjectOfferingRepository, ClassSectionSubjectOfferingRepository>();
        services.AddScoped<IClassSectionEligibilityValidationMessageRepository, ClassSectionEligibilityValidationMessageRepository>();
        services.AddScoped<ICourseCurriculumAssignmentRepository, CourseCurriculumAssignmentRepository>();


        services.AddScoped<IApplicableCurriculumQueryService, ApplicableCurriculumQueryService>();
        
        // Domain services
        services.AddScoped<ScheduleConflictDetector>();
        
        // Application services
        services.AddScoped<ConflictDetectionHelper>();

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
            try
            {
                // Check if it's SmartEnum<T> (int value) or SmartEnum<T, TValue> (custom value type)
                var baseType = smartEnumType.BaseType;
                while (baseType != null && baseType.IsGenericType)
                {
                    var genericTypeDef = baseType.GetGenericTypeDefinition();

                    if (genericTypeDef == typeof(SmartEnum<>))
                    {
                        // SmartEnum<T> with int values - use SmartEnumByValueTypeHandler
                        var handlerType = typeof(SmartEnumByValueTypeHandler<>).MakeGenericType(smartEnumType);
                        var handler = Activator.CreateInstance(handlerType);
                        SqlMapper.AddTypeHandler(smartEnumType, (SqlMapper.ITypeHandler)handler!);
                        break;
                    }
                    else if (genericTypeDef == typeof(SmartEnum<,>))
                    {
                        // SmartEnum<T, TValue> with custom value type - use SmartEnumByNameTypeHandler
                        var handlerType = typeof(SmartEnumByNameTypeHandler<>).MakeGenericType(smartEnumType);
                        var handler = Activator.CreateInstance(handlerType);
                        SqlMapper.AddTypeHandler(smartEnumType, (SqlMapper.ITypeHandler)handler!);
                        break;
                    }

                    baseType = baseType.BaseType;
                }
            }
            catch (ArgumentException)
            {
                // Skip SmartEnum types that violate generic constraints (e.g., sealed classes with custom value types)
                // These types cannot be used with the standard Dapper type handlers from Ardalis.SmartEnum.Dapper
                continue;
            }
            catch (TypeLoadException)
            {
                // Skip types that cannot be loaded or instantiated
                continue;
            }
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
