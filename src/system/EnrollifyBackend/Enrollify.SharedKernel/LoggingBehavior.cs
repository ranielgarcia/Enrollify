using System.Diagnostics;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.SharedKernel;

/// <summary>
/// Adds logging for all requests in MediatR pipeline.
/// Configure by adding the service with a scoped lifetime
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
  : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    Guard.Against.Null(request);

    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation("Handling {RequestName} with {@Request}", typeof(TRequest).Name, request);
    }

    var sw = Stopwatch.StartNew();

    var response = await next();

    sw.Stop();

    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation("Handled {RequestName} with {Response} in {ElapsedMilliseconds} ms",
        typeof(TRequest).Name, response, sw.ElapsedMilliseconds);
    }

    return response;
  }
}
