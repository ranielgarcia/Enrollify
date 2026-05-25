using Ardalis.Result;
using Enrollify.Application.Features.Departments.Specifications;
using Enrollify.Application.Features.Departments.DTOs;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Departments.Queries;

public class ListDepartmentsQuery : IRequest<Result<List<DepartmentDto>>>
{
}

public class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, Result<List<DepartmentDto>>>
{
    private readonly IReadRepository<Department> _readRepository;

    public ListDepartmentsQueryHandler(IReadRepository<Department> readRepository)
    {
        _readRepository = readRepository;
    }
    public async Task<Result<List<DepartmentDto>>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListDepartmentsWithAllNavigationSpec();
        var departments = await _readRepository.ListAsync(spec, cancellationToken);

        var toReturn = departments
            .Select(DepartmentDto.FromEntity)
            .ToList();

        return toReturn;
    }
}

