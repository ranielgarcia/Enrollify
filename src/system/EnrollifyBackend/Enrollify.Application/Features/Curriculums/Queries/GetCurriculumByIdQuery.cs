using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Application.Features.Curriculums.DTOs;

namespace Enrollify.Application.Features.Curriculums.Queries;

public record GetCurriculumByIdQuery(int Id) : IRequest<Result<CurriculumDetailDto>>;

public class GetCurriculumByIdQueryHandler(IReadRepository<Curriculum> readRepository)
    : IRequestHandler<GetCurriculumByIdQuery, Result<CurriculumDetailDto>>
{
    public async Task<Result<CurriculumDetailDto>> Handle(GetCurriculumByIdQuery query, CancellationToken cancellationToken)
    {
        var spec = new GetCurriculumByIdSpec(CurriculumId.From(query.Id));
        var projectionSpec = new CurriculumToCurriculumDetailDtoProjectionSpec();
        var combinedSpec = spec.WithProjectionOf(projectionSpec);

        var curriculumDto = await readRepository.FirstOrDefaultAsync(combinedSpec, cancellationToken);
        return curriculumDto != null
            ? Result<CurriculumDetailDto>.Success(curriculumDto)
            : Result.NotFound($"Curriculum with an ID of {query.Id} not found");
    }
}
