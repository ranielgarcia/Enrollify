using Ardalis.Result;
using Enrollify.Application.Features.Departments.Specifications;
using Enrollify.Application.Features.Departments.DTOs;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Departments.Features;

public class ListDepartmentsQuery : IQuery<Result<List<DepartmentDTO>>>
{
}

public class ListDepartmentsQueryHandler : IQueryHandler<ListDepartmentsQuery, Result<List<DepartmentDTO>>>
{
    private readonly IReadRepository<Department> _readRepository;

    public ListDepartmentsQueryHandler(IReadRepository<Department> readRepository)
    {
        _readRepository = readRepository;
    }
    public async ValueTask<Result<List<DepartmentDTO>>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListDepartmentsWithAllNavigationSpec();
        var departments = await _readRepository.ListAsync(spec, cancellationToken);

        var toReturn = departments
            .Select(DepartmentDTO.FromEntity)
            .ToList();

        return toReturn;
    }
}

