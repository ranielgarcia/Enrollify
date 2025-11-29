using Ardalis.Specification;

namespace Enrollify.Core.Aggregates.RoleAggregate.Specifications;

public class RolesByIdsSpec : Specification<Role>
{
    public RolesByIdsSpec(IEnumerable<RoleId> roleIds) =>
        Query
            .Include(r => r.RolePermissions)
            .Where(r => roleIds.Contains(r.Id));
}
