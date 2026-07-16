using Enrollify.Application.Behaviors;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.Validators;
using Enrollify.Application.Features.Users.Queries;
using Enrollify.Core.Authentication;
using Enrollify.Infrastructure;
using Enrollify.Infrastructure.Data;
using Enrollify.IntegrationTests.Helpers;
using Enrollify.SharedKernel;
using FluentValidation;
using FluentValidation.Internal;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// Test fixture for Application layer integration tests.
/// Provides access to IMediator, DbContext, and services for testing commands, queries, and services.
/// Each test class gets its own isolated database with fresh migrations.
/// </summary>
public class ApplicationTestFixture : IAsyncLifetime
{
    private TestContainersManager _containersManager = null!;
    private string _sqlConnectionString = null!;
    private IServiceProvider _serviceProvider = null!;

    public IServiceProvider Services => _serviceProvider;
    public string SqlConnectionString => _sqlConnectionString;
    public string AzuriteBlobConnectionString => _containersManager.AzuriteBlobConnectionString;

    public async Task InitializeAsync()
    {
        // FastEndpoints (used by WebApiTestFixture) sets ValidatorOptions.Global.PropertyNameResolver
        // to camelCase when its middleware is initialized. Reset it to PascalCase here so that
        // Application-layer validators produce PascalCase property names consistent with C# conventions.
        ValidatorOptions.Global.PropertyNameResolver = (_, memberInfo, expression) =>
        {
            if (memberInfo is not null)
                return memberInfo.Name;

            if (expression is not null)
            {
                var chain = PropertyChain.FromExpression(expression);
                if (chain.Count > 0) return chain.ToString();
            }

            return null;
        };

        // Get shared testcontainers instance
        _containersManager = await TestContainersManager.GetInstanceAsync();

        // Create a unique database for this test class
        _sqlConnectionString = await _containersManager.CreateDatabaseAsync();

        // Build service collection
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add configuration
        var configuration = TestConfigurationBuilder.BuildForApplicationLayer(
            _sqlConnectionString,
            _containersManager.AzuriteBlobConnectionString
        );
        services.AddSingleton<IConfiguration>(configuration);

        // Get a logger for infrastructure registration
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ApplicationTestFixture>();

        // Build a ConfigurationManager-like instance for Infrastructure registration
        var configManager = new ConfigurationManager();
        configManager.AddConfiguration(configuration);

        // Add Infrastructure services (DbContext, repositories, etc.)
        services.AddInfrastructureServices(configManager, logger, isDevelopment: true);

        // Wolverine's IDbContextOutbox is only registered when ConfigureWolverine is called
        // (which requires a running host with SQL Server persistence). For integration tests
        // we register a no-op stub so handlers that depend on IUnitOfWork / IDomainEventBus
        // can resolve without needing the full Wolverine runtime.
        services.AddScoped<IDbContextOutbox, TestDbContextOutbox>();

        // Register test-specific services
        services.AddSingleton<ICurrentUserAccessor, TestCurrentUserAccessor>();

        // Add MediatR with behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GetUserByEmailQuery>(); // Application
            cfg.RegisterServicesFromAssemblyContaining<EnrollifyDbContext>(); // Infrastructure (non-static)

            // Register pipeline behaviors (order matters - they run in registration order)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation validators
        // Use Scoped lifetime to support validators with constructor dependencies (e.g., IReadRepository<T>)
        services.AddValidatorsFromAssemblyContaining<CreateClassSectionValidator>(ServiceLifetime.Scoped);

        // Build service provider
        _serviceProvider = services.BuildServiceProvider();

        Console.WriteLine("[ApplicationTestFixture] Initialized with isolated database.");
    }

    public Task DisposeAsync()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Console.WriteLine("[ApplicationTestFixture] Disposed.");
        // Note: We don't dispose _containersManager here as it's shared across all tests
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the IMediator instance for sending commands and queries.
    /// </summary>
    public IMediator GetMediator()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    /// <summary>
    /// Gets a scoped DbContext instance.
    /// </summary>
    public EnrollifyDbContext GetDbContext()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<EnrollifyDbContext>();
    }

    /// <summary>
    /// Creates a new service scope for accessing services with proper lifetime management.
    /// </summary>
    public IServiceScope CreateServiceScope()
    {
        return _serviceProvider.CreateScope();
    }

    /// <summary>
    /// Executes an action within a new service scope and disposes it automatically.
    /// </summary>
    public async Task ExecuteInScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = CreateServiceScope();
        await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Executes a function within a new service scope and returns a result.
    /// </summary>
    public async Task<T> ExecuteInScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = CreateServiceScope();
        return await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Sends a request (command or query) via Mediator within a new scope.
    /// </summary>
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return await ExecuteInScopeAsync(async sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            return await mediator.Send(request);
        });
    }

    /// <summary>
    /// Executes a database operation within a new scope.
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<EnrollifyDbContext, Task<T>> action)
    {
        return await ExecuteInScopeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<EnrollifyDbContext>();
            return await action(dbContext);
        });
    }
}
