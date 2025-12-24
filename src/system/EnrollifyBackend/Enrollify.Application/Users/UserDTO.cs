using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Users;

public class UserDTO : BaseDTO
{
    public UserId Id { get; set; }
    public UserEmail Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTimeOffset? LastLoginAt { get; set; }

    public static UserDTO FromUser(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(user.CreatedByUser),
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = BaseUserDTO.FromUser(user.UpdatedByUser),
            DeletedAt = user.DeletedAt,
            DeletedBy = BaseUserDTO.FromUser(user.DeletedByUser)
        };
    }
}
