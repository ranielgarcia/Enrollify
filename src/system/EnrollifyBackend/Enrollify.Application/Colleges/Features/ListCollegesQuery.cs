using Ardalis.Result;
using Enrollify.Application.Colleges.DTOs;
using Enrollify.Application.Colleges.Specifications;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Colleges.Features;

public class ListCollegesQuery : IQuery<Result<List<CollegeDTO>>>
{
}

public class ListCollegesQueryHandler : IQueryHandler<ListCollegesQuery, Result<List<CollegeDTO>>>
{
    private readonly IReadRepository<College> _collegeRepository;

    public ListCollegesQueryHandler(IReadRepository<College> collegeRepository)
    {
        _collegeRepository = collegeRepository;
    }
    public async ValueTask<Result<List<CollegeDTO>>> Handle(ListCollegesQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListCollegesOrderByNameSpec();
        var colleges = await _collegeRepository.ListAsync(spec, cancellationToken);
        var toReturn = colleges
            .Select(CollegeDTO.FromEntity).ToList();
        return Result.Success(toReturn);
    }
}