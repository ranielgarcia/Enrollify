namespace Enrollify.Core.Aggregates.RoleAggregate.Models;

public class PermissionForCreation
{
    public string Name { get; set; } = null!;
    public string Resource { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Description { get; set; } = null!;
}
