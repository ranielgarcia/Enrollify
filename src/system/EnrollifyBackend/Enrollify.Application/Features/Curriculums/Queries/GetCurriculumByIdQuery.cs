using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Application.Features.Curriculums.DTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Curriculums.Queries;

public record GetCurriculumByIdQuery(int Id) : IQuery<Result<CurriculumDetailDto>>;

public class GetCurriculumByIdQueryHandler(IReadRepository<Curriculum> readRepository)
    : IQueryHandler<GetCurriculumByIdQuery, Result<CurriculumDetailDto>>
{
    public async ValueTask<Result<CurriculumDetailDto>> Handle(GetCurriculumByIdQuery query, CancellationToken cancellationToken)
    {
        var spec = new GetCurriculumByIdSpec(CurriculumId.From(query.Id));
        var projectionSpec = new CurriculumToCurriculumDetailDto();
        var combinedSpec = spec.WithProjectionOf(projectionSpec);

        var curriculumDto = await readRepository.FirstOrDefaultAsync(combinedSpec, cancellationToken);
        return curriculumDto != null 
            ? Result<CurriculumDetailDto>.Success(curriculumDto) 
            : Result.NotFound($"Curriculum with an ID of {query.Id} not found");
    }
}
