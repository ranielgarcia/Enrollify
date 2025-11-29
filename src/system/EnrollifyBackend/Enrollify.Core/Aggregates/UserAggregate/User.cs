using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate.Events;
using Enrollify.Core.Aggregates.UserAggregate.Models;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.UserAggregate;

public class User : EntityBase<User, UserId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private readonly List<UserRoleAssignment> _roleAssignments = new();
    
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public IReadOnlyCollection<UserRoleAssignment> RoleAssignments => _roleAssignments.AsReadOnly();

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public static User Create(UserForCreation userForCreation, UserId createdBy)
    {
        var newUser = new User(); 

        newUser.UpdateFirstName(userForCreation.FirstName);
        newUser.UpdateLastName(userForCreation.LastName);
        newUser.UpdateEmail(userForCreation.Email);

        newUser.AuditInfo.SetCreatedBy(createdBy);

        newUser.RegisterDomainEvent(new UserCreatedEvent(newUser));

        return newUser;
    }

    public User UpdateFirstName(string newFirstName)
    {
        if (FirstName == newFirstName) return this;
        FirstName = Guard.Against.Null(newFirstName);
        return this;
    }

    public User UpdateLastName(string newLastName)
    {
        if (LastName == newLastName) return this;
        LastName = Guard.Against.Null(newLastName);
        return this;
    }

    public User UpdateEmail (string newEmail)
    {
        if (Email == newEmail) return this;
        Email = Guard.Against.Null(newEmail);
        return this;
    }

    public User UpdateLastLoginAt(DateTimeOffset? lastLoginAt)
    {
        //if (EqualityComparer<UserLastLoginAt?>.Default.Equals(LastLoginAt, lastLoginAt)) return this;
        if (lastLoginAt == LastLoginAt) return this;
        LastLoginAt = lastLoginAt;
        return this;
    }

    public User AssignRole(RoleId roleId, UserId assignBy, DateTime? expiresAt = null)
    {
        Guard.Against.Null(roleId);
        Guard.Against.Null(assignBy);

        if (_roleAssignments.Any(ra => ra.RoleId == roleId && !ra.IsExpired()))
        {
            return this; // Role already assigned and active
        }

        var assignment = new UserRoleAssignment(roleId, expiresAt);

        assignment.AuditInfo.SetCreatedBy(assignBy);

        _roleAssignments.Add(assignment);
        return this;
    }

    public User RemoveRole(RoleId roleId)
    {
        Guard.Against.Null(roleId);

        var assignment = _roleAssignments.FirstOrDefault(ra => ra.RoleId == roleId && !ra.IsExpired());
        if (assignment != null)
        {
            _roleAssignments.Remove(assignment);
        }

        return this;
    }

    public User UpdateRoleExpiration(RoleId roleId, DateTime? expiresAt)
    {
        Guard.Against.Null(roleId);

        var assignment = _roleAssignments.FirstOrDefault(ra => ra.RoleId == roleId && !ra.IsExpired());
        assignment?.UpdateExpiration(expiresAt);

        return this;
    }

    public IEnumerable<RoleId> GetActiveRoles()
    {
        return _roleAssignments
            .Where(ra => !ra.IsExpired())
            .Select(ra => ra.RoleId);
    }

    public bool HasRole(RoleId roleId)
    {
        return _roleAssignments.Any(ra => ra.RoleId == roleId && !ra.IsExpired());
    }


    protected User() { } // EF Core constructor

}
