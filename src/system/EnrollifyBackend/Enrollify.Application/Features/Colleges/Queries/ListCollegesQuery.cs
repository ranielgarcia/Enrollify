using Enrollify.Application.Features.Colleges.DTOs;
using Enrollify.Application.Features.Colleges.Specifications;

namespace Enrollify.Application.Features.Colleges.Queries;

public class ListCollegesQuery : IRequest<Result<List<CollegeDto>>>
{
}

public class ListCollegesQueryHandler : IRequestHandler<ListCollegesQuery, Result<List<CollegeDto>>>
{
    private readonly IReadRepository<College> _collegeRepository;

    public ListCollegesQueryHandler(IReadRepository<College> collegeRepository)
    {
        _collegeRepository = collegeRepository;
    }
    public async Task<Result<List<CollegeDto>>> Handle(ListCollegesQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListCollegesOrderByNameSpec();
        var colleges = await _collegeRepository.ListAsync(spec, cancellationToken);
        var toReturn = colleges
            .Select(CollegeDto.FromEntity).ToList();
        return Result.Success(toReturn);
    }
}
