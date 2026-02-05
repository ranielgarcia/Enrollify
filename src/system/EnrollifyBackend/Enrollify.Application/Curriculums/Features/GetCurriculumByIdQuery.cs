using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Specifications;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Curriculums.Features;

public record GetCurriculumByIdQuery(int Id) : IQuery<Result<CurriculumDetailDTO>>;

public class GetCurriculumByIdQueryHandler(IReadRepository<Curriculum> readRepository)
    : IQueryHandler<GetCurriculumByIdQuery, Result<CurriculumDetailDTO>>
{
    public async ValueTask<Result<CurriculumDetailDTO>> Handle(GetCurriculumByIdQuery query, CancellationToken cancellationToken)
    {
        var spec = new GetCurriculumByIdSpec(CurriculumId.From(query.Id));
        var projectionSpec = new CurriculumToCurriculumDetailDTO();
        var combinedSpec = spec.WithProjectionOf(projectionSpec);

        var curriculumDto = await readRepository.FirstOrDefaultAsync(combinedSpec, cancellationToken);
        return curriculumDto != null 
            ? Result<CurriculumDetailDTO>.Success(curriculumDto) 
            : Result.NotFound($"Curriculum with an ID of {query.Id} not found");
    }
}