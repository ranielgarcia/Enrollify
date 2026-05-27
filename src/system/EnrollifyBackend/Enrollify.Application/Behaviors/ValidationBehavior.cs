using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before the handler.
/// If no validator is registered for the request, execution continues normally.
/// Throws <see cref="ValidationException"/> when validation fails.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorsList = validators.ToList();
        var requestType = typeof(TRequest).Name;

        logger.LogInformation(
            "ValidationBehavior: Processing {RequestType}. Found {ValidatorCount} validator(s).",
            requestType, validatorsList.Count);

        if (validatorsList.Count == 0)
        {
            logger.LogWarning(
                "ValidationBehavior: No validators registered for {RequestType}. Skipping validation.",
                requestType);
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validatorsList.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            logger.LogWarning(
                "ValidationBehavior: Validation failed for {RequestType} with {ErrorCount} error(s).",
                requestType, failures.Count);
            throw new ValidationException(failures);
        }

        logger.LogInformation(
            "ValidationBehavior: Validation passed for {RequestType}.",
            requestType);

        return await next();
    }
}
