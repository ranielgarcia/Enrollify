namespace Enrollify.Core.Services.Authentication;

public interface ICurrentUserAccessor
{
    UserContext? GetCurrentUser();
}
