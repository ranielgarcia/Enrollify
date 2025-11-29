namespace Enrollify.Core.Aggregates.UserAggregate.Models;

public class UserForCreation
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
}
