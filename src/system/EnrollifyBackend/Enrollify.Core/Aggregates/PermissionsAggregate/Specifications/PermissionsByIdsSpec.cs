using Ardalis.Specification;

namespace Enrollify.Core.Aggregates.PermissionsAggregate.Specifications;

public class PermissionsByIdsSpec : Specification<Permission>
{
    public PermissionsByIdsSpec(IEnumerable<PermissionId> permissionIds)
        => Query
            .Where(p => permissionIds.Contains(p.Id));
}
