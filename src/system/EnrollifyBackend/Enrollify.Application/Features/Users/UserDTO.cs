using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.Users;

public class UserDto : BaseDto
{
    public UserId Id { get; set; }
    public UserEmail Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTimeOffset? LastLoginAt { get; set; }

    public static UserDto FromUser(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(user.CreatedByUser),
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = BaseUserDto.FromUser(user.UpdatedByUser),
            DeletedAt = user.DeletedAt,
            DeletedBy = BaseUserDto.FromUser(user.DeletedByUser)
        };
    }
}
