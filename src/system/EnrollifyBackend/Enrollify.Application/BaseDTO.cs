using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application;

public class BaseUserDTO
{
    public UserId Id { get; set; }
    public UserEmail Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public static BaseUserDTO? FromUser(User? user)
    {
        if (user == null) return null;
        return new BaseUserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }
}

public class BaseDTO
{
    public DateTimeOffset CreatedAt { get; set; }
    public BaseUserDTO? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public BaseUserDTO? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public BaseUserDTO? DeletedBy { get; set; }
    public bool IsActive { get; set; }

}
