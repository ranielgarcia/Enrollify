using Ardalis.Result;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Functional-style matcher for Result<T>. Invokes <paramref name="onOk"/> when status is Ok,
    /// otherwise invokes <paramref name="onNotFound"/> (for NotFound and all other statuses).
    /// </summary>
    public static TReturn Match<TValue, TReturn>(
      this Result<TValue> result,
      Func<TValue, TReturn> onOk,
      Func<Result<TValue>, TReturn> onNotFound)
    {
        return result.Status switch
        {
            ResultStatus.Ok => onOk(result.Value),
            ResultStatus.NotFound => onNotFound(result),
            _ => onNotFound(result)
        };
    }

    public static Result<TDestination> Map<TSource, TDestination>(this Result<TSource> result, Func<TSource, TDestination> func)
    {
        switch (result.Status)
        {
            case ResultStatus.Ok: return func(result);
            case ResultStatus.NotFound: return Result<TDestination>.NotFound();
            case ResultStatus.Unauthorized: return Result<TDestination>.Unauthorized();
            case ResultStatus.Forbidden: return Result<TDestination>.Forbidden();
            case ResultStatus.Invalid: return Result<TDestination>.Invalid(result.ValidationErrors);
            case ResultStatus.Error: return Result<TDestination>.Error(string.Join(", ", result.Errors.ToArray()));
            default:
                throw new NotSupportedException($"Result {result.Status} conversion is not supported.");
        }
    }

    /// <summary>
    /// Maps Result to TypedResults for endpoints that return Created, ValidationProblem, or ProblemHttpResult
    /// </summary>
    public static Results<Created<TResponse>, ValidationProblem, ProblemHttpResult> ToCreatedResult<TValue, TResponse>(
      this Result<TValue> result,
      Func<TValue, string> locationBuilder,
      Func<TValue, TResponse> mapResponse)
    {
        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Created(locationBuilder(result.Value), mapResponse(result.Value)),
            ResultStatus.Invalid => TypedResults.ValidationProblem(
              result.ValidationErrors
                .GroupBy(e => e.Identifier ?? string.Empty)
                .ToDictionary(
                  g => g.Key,
                  g => g.Select(e => e.ErrorMessage).ToArray()
                )
            ),
            _ => TypedResults.Problem(
              title: "Create failed",
              detail: string.Join("; ", result.Errors),
              statusCode: StatusCodes.Status400BadRequest)
        };
    }

    /// <summary>
    /// Maps Result to TypedResults for GetById endpoints that return Ok, NotFound, or ProblemHttpResult
    /// </summary>
    public static Results<Ok<TResponse>, NotFound, ProblemHttpResult> ToGetByIdResult<TValue, TResponse>(
      this Result<TValue> result,
      Func<TValue, TResponse> mapResponse)
    {
        return ToOkOrNotFoundResult(result, mapResponse, "Get");
    }

    /// <summary>
    /// Maps Result to TypedResults for Update endpoints that return Ok, NotFound, or ProblemHttpResult
    /// </summary>
    public static Results<Ok<TResponse>, NotFound, ProblemHttpResult> ToUpdateResult<TValue, TResponse>(
      this Result<TValue> result,
      Func<TValue, TResponse> mapResponse)
    {
        return ToOkOrNotFoundResult(result, mapResponse, "Update");
    }

    /// <summary>
    /// Maps Result to TypedResults for Delete endpoints that return NoContent, NotFound, or ProblemHttpResult
    /// </summary>
    public static Results<NoContent, NotFound, ProblemHttpResult> ToDeleteResult(
      this Result result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.NoContent(),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem(
              title: "Delete failed",
              detail: string.Join("; ", result.Errors),
              statusCode: StatusCodes.Status400BadRequest)
        };
    }

    /// <summary>
    /// Private helper method for Ok/NotFound result patterns
    /// </summary>
    private static Results<Ok<TResponse>, NotFound, ProblemHttpResult> ToOkOrNotFoundResult<TValue, TResponse>(
      Result<TValue> result,
      Func<TValue, TResponse> mapResponse,
      string operationName)
    {
        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(mapResponse(result.Value)),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem(
              title: $"{operationName} failed",
              detail: string.Join("; ", result.Errors),
              statusCode: StatusCodes.Status400BadRequest)
        };
    }

    /// <summary>
    /// Maps Result to TypedResults for endpoints that return Ok only (like List endpoints)
    /// </summary>
    public static Ok<TResponse> ToOkOnlyResult<TValue, TResponse>(
      this Result<TValue> result,
      Func<TValue, TResponse> mapResponse)
    {
        return TypedResults.Ok(mapResponse(result.Value));
    }
}