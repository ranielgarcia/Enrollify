using Ardalis.SmartEnum;
using Ardalis.SmartEnum.Dapper;
using Dapper;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Application.Features.Buildings;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.Colleges;
using Enrollify.Application.Features.CourseCurriculumAssignments;
using Enrollify.Application.Features.Courses;
using Enrollify.Application.Features.Curriculums;
using Enrollify.Application.Features.Departments;
using Enrollify.Application.Features.Notifications;
using Enrollify.Application.Features.Roles.Queries;
using Enrollify.Application.Features.Rooms;
using Enrollify.Application.Features.RoomTypes;
using Enrollify.Application.Features.SubjectEquivalences;
using Enrollify.Application.Features.Subjects;
using Enrollify.Application.Features.Teachers;
using Enrollify.Application.Features.Teachers.Storage;
using Enrollify.Core.Constants;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services;
using Enrollify.Core.Services.ClassSectionDataIntegrityValidation;
using Enrollify.Core.Services.NotificationServices;
using Enrollify.Core.Services.ScheduleConflictDetection;
using Enrollify.Infrastructure.Data;
using Enrollify.Infrastructure.Data.Dapper.Generated;
using Enrollify.Infrastructure.Data.Queries;
using Enrollify.Infrastructure.Persistence;
using Enrollify.Infrastructure.Repositories;
using Enrollify.Infrastructure.Services;
using Enrollify.Infrastructure.Services.NotificationServices;
using Enrollify.Infrastructure.Storage;
using Enrollify.SharedKernel;
using INotificationPublisher = Enrollify.Core.Services.NotificationServices.INotificationPublisher;

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

    // Auto register all SmartEnum Dapper type handlers from Enrollify.Core assembly
    // This handles regular SmartEnum types like PermissionScopeEnum, etc.
    RegisterSmartEnumTypeHandlers(typeof(PermissionScopeEnum).Assembly);

    // Manually register SmartFlagEnum types (bit-flag enums)
    // SmartFlagEnum requires a specific handler because it can hold multiple values
    SqlMapper.AddTypeHandler(new SmartFlagEnumDapperTypeHandler<PermissionEnum>());

    // Custom handler for DayOfWeekEnum (SmartEnum<T, string> with string values)
    // The auto-registration uses FromName() but our database stores values ("MON", "TUE", etc.)
    // This handler uses FromValue() to correctly parse the database values
    SqlMapper.AddTypeHandler(new DayOfWeekEnumDapperTypeHandler());


    // Register custom type handler for TimeSpan → TimeOnly conversion
    // This is required because Dapper doesn't have built-in support for converting
    // SQL Server TIME columns (TimeSpan) to .NET TimeOnly type
    SqlMapper.AddTypeHandler(new TimeSpanToTimeOnlyDapperTypeHandler());

    services.AddScoped<EventDispatchInterceptor>();
    services.AddScoped<PreSaveChangesInterceptor>();
    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

    services.AddDbContext<EnrollifyDbContext>((provider, options) =>
    {
      EventDispatchInterceptor eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();
      PreSaveChangesInterceptor preSaveChangesInterceptor = provider.GetRequiredService<PreSaveChangesInterceptor>();

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
    services
      .AddScoped<IClassSectionSubjectOfferingScheduleConflictRepository,
        ClassSectionSubjectOfferingScheduleConflictRepository>();
    services.AddScoped<IClassSectionValidationIssueRepository, ClassSectionValidationIssueRepository>();
    services.AddScoped<ICourseCurriculumAssignmentRepository, CourseCurriculumAssignmentRepository>();
    services.AddScoped<IClassSectionSchedulingStatsRepository, ClassSectionSchedulingStatsRepository>();
    services.AddScoped<INotificationRepository, NotificationRepository>();

    services.AddScoped<IApplicableCurriculumQueryService, ApplicableCurriculumQueryService>();
    services.AddScoped<IUserQueryService, UserQueryService>();

    // Notification Services
    services.AddScoped<INotificationPublisher, NotificationPublisher>();
    services.AddScoped<INotificationBus, NotificationBus>();

    // Domain services
    services.AddScoped<ClassSectionDataIntegrityValidator>();
    services.AddScoped<ClassScheduleConflictDetector>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }

  /// <summary>
  /// Registers Dapper type handlers for all SmartEnum types found in the specified assemblies.
  /// </summary>
  /// <param name="assemblies">Assemblies to scan for SmartEnum types. If none provided, scans the calling assembly.</param>
  public static void RegisterSmartEnumTypeHandlers(params Assembly[] assemblies)
  {
    if (assemblies == null || assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

    IEnumerable<Type> smartEnumTypes = assemblies
      .SelectMany(assembly => assembly.GetTypes())
      .Where(type => type is { IsClass: true, IsAbstract: false }
                     && IsSmartEnum(type));

    foreach (Type smartEnumType in smartEnumTypes)
      try
      {
        // Check if it's SmartEnum<T> (int value) or SmartEnum<T, TValue> (custom value type)
        Type? baseType = smartEnumType.BaseType;
        while (baseType != null && baseType.IsGenericType)
        {
          Type genericTypeDef = baseType.GetGenericTypeDefinition();

          if (genericTypeDef == typeof(SmartEnum<>))
          {
            // SmartEnum<T> with int values - use SmartEnumByValueTypeHandler
            Type handlerType = typeof(SmartEnumByValueTypeHandler<>).MakeGenericType(smartEnumType);
            object? handler = Activator.CreateInstance(handlerType);
            SqlMapper.AddTypeHandler(smartEnumType, (SqlMapper.ITypeHandler)handler!);
            break;
          }
          else if (genericTypeDef == typeof(SmartEnum<,>))
          {
            // SmartEnum<T, TValue> with custom value type - use SmartEnumByNameTypeHandler
            Type handlerType = typeof(SmartEnumByNameTypeHandler<>).MakeGenericType(smartEnumType);
            object? handler = Activator.CreateInstance(handlerType);
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

  private static bool IsSmartEnum(Type type)
  {
    Type? baseType = type.BaseType;
    while (baseType != null)
    {
      if (baseType.IsGenericType)
      {
        Type genericTypeDef = baseType.GetGenericTypeDefinition();
        if (genericTypeDef == typeof(SmartEnum<>) ||
            genericTypeDef == typeof(SmartEnum<,>))
          return true;
      }

      baseType = baseType.BaseType;
    }

    return false;
  }
}
