using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate.Events;
using Enrollify.Core.Aggregates.UserAggregate.Models;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.UserAggregate;

public class User : EntityBase<User, UserId>, IAggregateRoot, IAuditable
{
    private readonly List<UserRoleAssignment> _roleAssignments = new();
    
    public UserEmail Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public IReadOnlyCollection<UserRoleAssignment> RoleAssignments => _roleAssignments.AsReadOnly();


    public DateTimeOffset CreatedAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public User? CreatedByUser { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public UserId? UpdatedBy { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public UserId? DeletedBy { get; private set; }
    public User? DeletedByUser { get; private set; }
    public bool IsActive { get; private set; }


    public static User Create(UserForCreation userForCreation, UserId createdBy)
    {
        var newUser = new User();

        newUser.FirstName = Guard.Against.NullOrEmpty(userForCreation.FirstName);
        newUser.LastName = Guard.Against.NullOrEmpty(userForCreation.LastName);
        newUser.Email = UserEmail.From(Guard.Against.NullOrEmpty(userForCreation.Email));

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

    public User UpdateEmail (UserEmail newEmail)
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

        var assignment = new UserRoleAssignment(roleId, assignBy, expiresAt);

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
        return _roleAssignments.Any(ra => ra.RoleId == roleId && !ra.IsExpired() && !ra.IsActive);
    }


    protected User() { } // EF Core constructor

}
