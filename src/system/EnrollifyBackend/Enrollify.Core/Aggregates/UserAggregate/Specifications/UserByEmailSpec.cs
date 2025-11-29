using Ardalis.Specification;

namespace Enrollify.Core.Aggregates.UserAggregate.Specifications;

public class UserByEmailSpec : Specification<User>
{
    public UserByEmailSpec(UserEmail email) =>
        Query
            .Include(u => u.RoleAssignments)
            .Where(u => u.Email == email);
}
