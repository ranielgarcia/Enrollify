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
    public UserId CreatedBy { get; set; }
    public BaseUserDTO? CreatedByUser { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public UserId? UpdatedBy { get; set; }
    public BaseUserDTO? UpdatedByUser { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public UserId? DeletedBy { get; set; }
    public BaseUserDTO? DeletedByUser { get; set; }
    public bool IsActive { get; set; }

}
