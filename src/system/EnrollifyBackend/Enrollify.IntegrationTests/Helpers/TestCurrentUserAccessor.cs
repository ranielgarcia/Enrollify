using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Authentication;

namespace Enrollify.IntegrationTests.Helpers;

/// <summary>
/// Test implementation of ICurrentUserAccessor that returns a fixed test system user.
/// </summary>
public class TestCurrentUserAccessor : ICurrentUserAccessor
{
    // Use a system user ID of 1 for all test operations
    private static readonly UserId SystemUserId = UserId.From(1);

    public UserContext? GetCurrentUser()
    {
        // Return a minimal user context for test purposes
        // The interceptor only needs the Id property
        return new UserContext
        {
            // Use reflection to set the private Id property
        }.WithId(SystemUserId);
    }
}

internal static class UserContextExtensions
{
    public static UserContext WithId(this UserContext context, UserId id)
    {
        typeof(UserContext).GetProperty(nameof(UserContext.Id))!
            .SetValue(context, id);
        return context;
    }
}
