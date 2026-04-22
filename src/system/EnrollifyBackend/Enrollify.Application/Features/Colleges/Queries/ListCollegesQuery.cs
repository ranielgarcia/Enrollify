using Ardalis.Result;
using Enrollify.Application.Features.Colleges.DTOs;
using Enrollify.Application.Features.Colleges.Specifications;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Colleges.Queries;

public class ListCollegesQuery : IQuery<Result<List<CollegeDto>>>
{
}

public class ListCollegesQueryHandler : IQueryHandler<ListCollegesQuery, Result<List<CollegeDto>>>
{
    private readonly IReadRepository<College> _collegeRepository;

    public ListCollegesQueryHandler(IReadRepository<College> collegeRepository)
    {
        _collegeRepository = collegeRepository;
    }
    public async ValueTask<Result<List<CollegeDto>>> Handle(ListCollegesQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListCollegesOrderByNameSpec();
        var colleges = await _collegeRepository.ListAsync(spec, cancellationToken);
        var toReturn = colleges
            .Select(CollegeDto.FromEntity).ToList();
        return Result.Success(toReturn);
    }
}
