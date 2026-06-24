using Enrollify.Application.Behaviors;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.Validators;
using Enrollify.Application.Features.Users.Queries;
using Enrollify.Infrastructure;
using Enrollify.Infrastructure.Data;
using Enrollify.SharedKernel;
using FluentValidation;
using MediatR;

namespace Enrollify.WebAPI.Plumbing;

public static class MediatorConfig
{
    // Should be called from ServiceConfigs.cs, not Program.cs
    public static IServiceCollection AddMediatR(this IServiceCollection services,
      Microsoft.Extensions.Logging.ILogger logger)
    {
        logger.LogInformation("Registering MediatR and Behaviors");

        // Register MediatR from assemblies containing marker types
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GetUserByEmailQuery>(); // Application
            cfg.RegisterServicesFromAssemblyContaining<EnrollifyDbContext>(); // Infrastructure (non-static)
            cfg.RegisterServicesFromAssemblyContaining<Program>(); // WebAPI (Program is non-static in .NET 6+)

            // Register pipeline behaviors (order matters - they run in registration order)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register all FluentValidation validators from the Application assembly
        // Use Scoped lifetime to support validators with constructor dependencies (e.g., IReadRepository<T>)
        services.AddValidatorsFromAssemblyContaining<CreateClassSectionValidator>(ServiceLifetime.Scoped);

        return services;
    }
}
