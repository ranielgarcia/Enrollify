using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Services.Authentication;
using Microsoft.Identity.Web;

namespace Enrollify.WebAPI.Authentication;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserContextMiddleware> _logger;

    public UserContextMiddleware(
        RequestDelegate next,
        ILogger<UserContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        IUserContextService userContextService)
    {
        var cancellationToken = httpContext.RequestAborted;
        //var userEmail = httpContext?.User?.Identity?.Name;
        var userEmail = GetEmailClaim(httpContext);

        if (httpContext != null && !string.IsNullOrWhiteSpace(userEmail))
        {
            var email = GetEmailClaim(httpContext);

            var userContext = await userContextService.GetUserContextByEmail(UserEmail.From(email), cancellationToken);

            if (userContext != null)
            {
                SetHttpContext(userContext, httpContext);
            }
        }

        await _next(httpContext!);
    }

    private void SetHttpContext(UserContext currentUserContext, HttpContext httpContext)
    {
        httpContext.Items[UserContext.Key] = currentUserContext;
    }
    private static string? GetEmailClaim(HttpContext httpContext) =>
        httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.PreferredUserName)?.Value;

    //private static string? GetNameClaim(HttpContext httpContext) =>
    //    httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.Name)?.Value;

    //private static string? GetObjectIdClaim(HttpContext httpContext) =>
    //    httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.ObjectId)?.Value;

}
