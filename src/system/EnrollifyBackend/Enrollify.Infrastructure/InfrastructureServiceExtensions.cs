using Ardalis.SmartEnum;
using Ardalis.SmartEnum.Dapper;
using Dapper;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Application.Features.Buildings;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClientDataInvalidations;
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
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services;
using Enrollify.Core.Services.ClassSectionDataIntegrityValidation;
using Enrollify.Core.Services.ClientDataInvalidation;
using Enrollify.Core.Services.NotificationServices;
using Enrollify.Core.Services.NotificationServices.Models;
using Enrollify.Core.Services.ScheduleConflictDetection;
using Enrollify.Infrastructure.Data;
using Enrollify.Infrastructure.Data.Dapper.Generated;
using Enrollify.Infrastructure.Data.Queries;
using Enrollify.Infrastructure.Persistence;
using Enrollify.Infrastructure.RealTime;
using Enrollify.Infrastructure.Repositories;
using Enrollify.Infrastructure.Services;
using Enrollify.Infrastructure.Services.ClientDataInvalidation;
using Enrollify.Infrastructure.Services.NotificationServices;
using Enrollify.Infrastructure.Storage;
using Enrollify.SharedKernel;
using JasperFx.Core;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.SqlServer;
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
    string? connectionString = config.GetConnectionString("cleanarchitecture")
                               ?? config.GetConnectionString("DefaultConnection")
                               ?? config.GetConnectionString("SqliteConnection");
    Guard.Against.Null(connectionString);

    services.AddTransient<IDbConnectionFactory, SqlConnectionFactory>();

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
    services.AddScoped<IDomainEventBus, DomainEventBus>();

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
    services
      .AddScoped<Application.Features.RoomScheduling.Repositories.IRoomScheduleReadRepository,
        RoomScheduleReadRepository>();
    services.AddScoped<IClassSectionValidationIssueRepository, ClassSectionValidationIssueRepository>();
    services.AddScoped<ICourseCurriculumAssignmentRepository, CourseCurriculumAssignmentRepository>();
    services.AddScoped<IClassSectionSchedulingStatsRepository, ClassSectionSchedulingStatsRepository>();
    services.AddScoped<INotificationRepository, NotificationRepository>();

    services.AddScoped<IApplicableCurriculumQueryService, ApplicableCurriculumQueryService>();
    services.AddScoped<IUserQueryService, UserQueryService>();

    // Notification Services
    services.AddScoped<INotificationPublisher, NotificationPublisher>();
    services.AddScoped<INotificationBus, NotificationBus>();

    // Client Data Invalidation
    services.AddScoped<IClientDataInvalidationDispatcher, ClientDataInvalidationDispatcher>();

    // Real-time notification push (SignalR)
    services.AddSignalR(options =>
    {
      // Surface hub/connection errors to clients in development so a 1006
      // "no reason given" close carries an actual diagnostic message.
      options.EnableDetailedErrors = isDevelopment;
      // Keep the WebSocket transport alive so idle connections aren't torn
      // down. Client default server timeout is 30s; a 15s ping stays inside it.
      options.KeepAliveInterval = TimeSpan.FromSeconds(15);
      options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
      options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    });
    services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
    services.AddScoped<IRealTimeNotificationSender, SignalRRealTimeNotificationSender>();
    services.AddScoped<IRealTimeClientDataInvalidationDispatcher, SignalRRealTimeClientDataInvalidationDispatcher>();

    // Domain services
    services.AddScoped<ClassSectionDataIntegrityValidator>();
    services.AddScoped<ClassScheduleConflictDetector>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }

  public static IHostBuilder ConfigureWolverine(this IHostBuilder hostBuilder, ConfigurationManager config, ILogger logger, bool isDevelopment = false)
  {
    hostBuilder.UseWolverine(opts =>
    {
      // Read connection string INSIDE the lambda so it is evaluated during IHostBuilder.Build(),
      // after WebApplicationFactory (integration tests) has applied its ConfigureAppConfiguration
      // overrides. Reading it eagerly before the lambda would capture the appsettings.json value
      // instead of the test container connection string.
      string? connectionString = config.GetConnectionString("cleanarchitecture")
                                 ?? config.GetConnectionString("DefaultConnection")
                                 ?? config.GetConnectionString("SqliteConnection");

      Guard.Against.Null(connectionString);

      opts.UseRuntimeCompilation();
      opts.CodeGeneration.AlwaysUseServiceLocationFor<EnrollifyDbContext>();
      opts.CodeGeneration.AlwaysUseServiceLocationFor<IMediator>();
      // Right here, tell Wolverine to make every handler "sticky"
      opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

      opts.Discovery.IncludeAssembly(typeof(OnNotificationCreatedEventHandler).Assembly);

      // Console.WriteLine(opts.DescribeHandlerMatch(typeof(OnNotificationCreatedEventHandler)));
      opts.PersistMessagesWithSqlServer(connectionString);
      opts.UseEntityFrameworkCoreTransactions();

      // opts.Policies.UseDurableLocalQueues();
      opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
      opts.Policies.UseDurableInboxOnAllListeners();

      // ClassSectionCreatedEvent kicks off a cascading local-message chain
      // (OnClassSectionCreatedEventHandler -> RefreshClassSectionValidationIssuesRequestedEvent ->
      // OnRefreshClassSectionValidationIssuesRequestedEventHandler -> ComputeAndGetValidationIssuesForClassSection).
      // Because of the global UseDurableOutboxOnAllSendingEndpoints() policy above, this local queue
      // would otherwise run in EndpointMode.Durable, which processes local cascading messages
      // synchronously as part of SaveChangesAndFlushMessagesThenCommitAsync - making callers like
      // CreateClassSection/BulkInitializeClassSectionsForAcademicYear block until the whole
      // validation-issue recomputation finishes. Routing these two message types to a dedicated
      // BufferedInMemory queue restores true async, fire-and-forget local dispatch so those commands
      // return as soon as the class section(s) are committed.
      // Trade-off: this queue is intentionally non-durable - a pending refresh is dropped (not
      // retried) if the process crashes between commit and processing. That's acceptable here
      // because UpdateClassSection, UpdateClassSectionSubjectOffering, AddMultipleSchedulesToOffering,
      // and RemoveScheduleFromOffering already re-trigger RefreshClassSectionValidationIssuesRequestedEvent
      // independently, so a missed refresh self-heals on the next relevant write.
      /*opts.LocalQueue("class-section-validation").BufferedInMemory();
      opts.Publish(x =>
      {
        x.Message<ClassSectionCreatedEvent>();
        x.Message<RefreshClassSectionValidationIssuesRequestedEvent>();
        x.ToLocalQueue("class-section-validation");
      });*/

      if (isDevelopment)
      {
        opts.Durability.Mode = DurabilityMode.Solo;
      }

      opts.BatchMessagesOf<NotificationCreatedEvent>(batching =>
      {
        batching.BatchSize = 5;
        batching.LocalExecutionQueueName = "Notifications";
        batching.TriggerTime = 2.Seconds();
      });

      opts.Policies.AutoApplyTransactions();
    });
    return hostBuilder;
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
