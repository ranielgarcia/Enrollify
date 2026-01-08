using Ardalis.Result;
using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Specifications;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Curriculums.Features;

public record GetCurriculumByIdQuery(int Id) : IQuery<Result<CurriculumDTO>>;

public class GetCurriculumByIdQueryHandler(IReadRepository<Curriculum> readRepository)
    : IQueryHandler<GetCurriculumByIdQuery, Result<CurriculumDTO>>
{
    public async ValueTask<Result<CurriculumDTO>> Handle(GetCurriculumByIdQuery query, CancellationToken cancellationToken)
    {
        var spec = new GetCurriculumByIdSpec(CurriculumId.From(query.Id));
        var curriculum = await readRepository.FirstOrDefaultAsync(spec, cancellationToken);
        return curriculum != null 
            ? Result<CurriculumDTO>.Success(CurriculumDTO.FromEntity(curriculum)) 
            : Result.NotFound($"Curriculum with an ID of {query.Id} not found");
    }
}