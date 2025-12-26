using Ardalis.Result;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<DepartmentId>> Create (Department newDepartment, CancellationToken cancellationToken);
    Task<Result<DepartmentId>> Update (Department newDepartment, CancellationToken cancellationToken);
    Task<Result> Delete (DepartmentId id, CancellationToken cancellationToken);
}
