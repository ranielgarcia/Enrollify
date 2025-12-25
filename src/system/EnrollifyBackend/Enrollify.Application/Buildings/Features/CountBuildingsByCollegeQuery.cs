using Ardalis.Result;
using Enrollify.Application.Buildings.Specifications;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public class CountBuildingsByCollegeQuery : IQuery<Result<int>>
{
    public CollegeId CollegeId { get; set; }
}

public class CountBuildingsByCollegeQueryHandler(IReadRepository<Building> buildingRepository)
    : IQueryHandler<CountBuildingsByCollegeQuery, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CountBuildingsByCollegeQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListBuildingsByCollegeSpec(query.CollegeId);
        var count = await buildingRepository.CountAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}