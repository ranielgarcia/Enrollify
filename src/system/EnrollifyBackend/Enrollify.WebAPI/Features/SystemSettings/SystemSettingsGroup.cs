namespace Enrollify.WebAPI.Features.SystemSettings;

public class SystemSettingsGroup : Group
{
    public SystemSettingsGroup()
    {
        Configure("system-settings", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
