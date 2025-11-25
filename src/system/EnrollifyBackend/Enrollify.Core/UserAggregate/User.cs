using Ardalis.GuardClauses;
using Enrollify.SharedKernel;

namespace Enrollify.Core.UserAggregate;

public class User : EntityBase<User, UserId>, IAggregateRoot
{
    private User() { } // EF Core constructor

    public User (UserFirstName firstName, UserLastName lastName, UserEmail email)
    {

    }

    public UserFirstName FirstName { get; private set; }
    public UserLastName LastName { get; private set; }
    public UserEmail Email { get; private set; }



    public User UpdateFirstName(UserFirstName newFirstName)
    {
        if (FirstName == newFirstName) return this;
        FirstName = Guard.Against.Null(newFirstName);
        return this;
    }

    public User UpdateLastName(UserLastName newLastName)
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
}
