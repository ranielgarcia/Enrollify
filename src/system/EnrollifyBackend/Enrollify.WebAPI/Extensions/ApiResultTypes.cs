namespace Enrollify.WebAPI.Extensions;

/// <summary>
/// Centralized result type for Create endpoints.
/// Add or remove constituent types here to update all Create endpoints at once.
/// </summary>
public readonly struct CreatedApiResult<TResponse>(IResult inner) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext) => inner.ExecuteAsync(httpContext);

    public static implicit operator CreatedApiResult<TResponse>(Created<TResponse> r) => new(r);
    public static implicit operator CreatedApiResult<TResponse>(ValidationProblem r) => new(r);
    public static implicit operator CreatedApiResult<TResponse>(Conflict<string[]> r) => new(r);
    public static implicit operator CreatedApiResult<TResponse>(ProblemHttpResult r) => new(r);
}

/// <summary>
/// Centralized result type for Get/Update endpoints (Ok + NotFound pattern).
/// Add or remove constituent types here to update all Get/Update endpoints at once.
/// </summary>
public readonly struct OkOrNotFoundApiResult<TResponse>(IResult inner) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext) => inner.ExecuteAsync(httpContext);

    public static implicit operator OkOrNotFoundApiResult<TResponse>(Ok<TResponse> r) => new(r);
    public static implicit operator OkOrNotFoundApiResult<TResponse>(NotFound r) => new(r);
    public static implicit operator OkOrNotFoundApiResult<TResponse>(ValidationProblem r) => new(r);
    public static implicit operator OkOrNotFoundApiResult<TResponse>(Conflict<string[]> r) => new(r);
    public static implicit operator OkOrNotFoundApiResult<TResponse>(ProblemHttpResult r) => new(r);
}

/// <summary>
/// Centralized result type for Delete endpoints.
/// Add or remove constituent types here to update all Delete endpoints at once.
/// </summary>
public readonly struct DeleteApiResult(IResult inner) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext) => inner.ExecuteAsync(httpContext);

    public static implicit operator DeleteApiResult(NoContent r) => new(r);
    public static implicit operator DeleteApiResult(NotFound r) => new(r);
    public static implicit operator DeleteApiResult(ValidationProblem r) => new(r);
    public static implicit operator DeleteApiResult(Conflict<string[]> r) => new(r);
    public static implicit operator DeleteApiResult(ProblemHttpResult r) => new(r);
}
