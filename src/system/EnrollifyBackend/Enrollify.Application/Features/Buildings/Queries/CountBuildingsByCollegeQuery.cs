using Ardalis.Result;
using Enrollify.Application.Features.Buildings.Specifications;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Buildings.Queries;

public class CountBuildingsByCollegeQuery : IRequest<Result<int>>
{
    public CollegeId CollegeId { get; set; }
}

public class CountBuildingsByCollegeQueryHandler(IReadRepository<Building> buildingRepository)
    : IRequestHandler<CountBuildingsByCollegeQuery, Result<int>>
{
    public async Task<Result<int>> Handle(CountBuildingsByCollegeQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListBuildingsByCollegeSpec(query.CollegeId);
        var count = await buildingRepository.CountAsync(spec, cancellationToken);
        return Result<int>.Success(count);
    }
}
