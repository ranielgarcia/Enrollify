namespace Enrollify.WebAPI.Features.Notifications;

public class NotificationEndpointGroup : Group
{
  public NotificationEndpointGroup()
  {
    Configure("notifications", ep =>
    {
      ep.Description(x => x.Produces(401));
    });
  }
}
