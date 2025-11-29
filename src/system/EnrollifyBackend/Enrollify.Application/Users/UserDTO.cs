using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core;

namespace Enrollify.Application.Users;

public class UserDTO
{
    public UserId Id { get; set; }
    public UserEmail Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTimeOffset? LastLoginAt { get; set; }
    public AuditInfoDTO AuditInfo { get; set; } = new AuditInfoDTO();

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
            AuditInfo = AuditInfoDTO.FromAuditInfo(user.AuditInfo)
        };
    }
}
