using Enrollify.Core.Authentication;

namespace Enrollify.WebAPI.Authentication;

public class HttpUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserContext? GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.Items[UserContext.Key] as UserContext;
    }
}
