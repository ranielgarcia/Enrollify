namespace Enrollify.WebAPI.Authorization.Notifications;

public static class NotificationAuthorizationPolicyRegistration
{
  public static IServiceCollection AddNotificationAuthorizationPolicyHandlers(this IServiceCollection services)
  {
    services.AddScoped<IAuthorizationHandler, HasViewNotificationPermissionHandler>();
    services.AddScoped<IAuthorizationHandler, HasUpdateNotificationPermissionHandler>();
    return services;
  }

  public static void AddNotificationAuthorizationPolicies(this AuthorizationOptions options)
  {
    options.AddPolicy(PolicyName.HasViewNotificationPermission, policyBuilder =>
      policyBuilder.AddRequirements(new HasViewNotificationPermission()));

    options.AddPolicy(PolicyName.HasUpdateNotificationPermission, policyBuilder =>
      policyBuilder.AddRequirements(new HasUpdateNotificationPermission()));
  }
}
