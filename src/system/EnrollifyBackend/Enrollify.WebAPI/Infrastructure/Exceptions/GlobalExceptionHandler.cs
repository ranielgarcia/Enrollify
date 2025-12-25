using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Enrollify.WebAPI.Infrastructure.Exceptions;


// https://medium.com/@AntonAntonov88/handling-errors-with-iexceptionhandler-in-asp-net-core-8-0-48c71654cc2e
public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const string UnhandledExceptionMsg = "An unhandled exception has occurred while executing the request.";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        exception.AddErrorCode();

        //If your logger logs DiagnosticsTelemetry, you should remove the string below to avoid the exception being logged twice.
        //logger.LogError(exception, exception is YourAppException ? exception.Message : UnhandledExceptionMsg);
        logger.LogError(exception, UnhandledExceptionMsg);

        var problemDetails = CreateProblemDetails(context, exception);
        var json = ToJson(problemDetails);

        const string contentType = "application/problem+json";
        context.Response.ContentType = contentType;
        await context.Response.WriteAsync(json, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(in HttpContext context, in Exception exception)
    {
        var errorCode = exception.GetErrorCode();
        var statusCode = context.Response.StatusCode;

        if (context.RequestAborted.IsCancellationRequested)
        {
            // Client Closed Request
            statusCode = 499;
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            return new ProblemDetails
            {
                Status = statusCode
            };
        }

        var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);
        if (string.IsNullOrEmpty(reasonPhrase))
        {
            reasonPhrase = UnhandledExceptionMsg;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = reasonPhrase,
            Extensions =
            {
                [ExceptionExtensions.ErrorCodeKey] = errorCode
            }
        };

        if (!env.IsDevelopment())
        {
            return problemDetails;
        }

        problemDetails.Detail = exception.ToString();
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["data"] = exception.Data;

        return problemDetails;
    }

    private string ToJson(in ProblemDetails problemDetails)
    {
        try
        {
            return JsonSerializer.Serialize(problemDetails, SerializerOptions);
        }
        catch (Exception ex)
        {
            const string msg = "An exception has occurred while serializing error to JSON";
            logger.LogError(ex, msg);
        }

        return string.Empty;
    }
}
