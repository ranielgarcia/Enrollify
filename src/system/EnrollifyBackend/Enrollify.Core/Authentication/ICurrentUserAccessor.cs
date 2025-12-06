namespace Enrollify.Core.Authentication;

public interface ICurrentUserAccessor
{
    UserContext? GetCurrentUser();
}
