using Ardalis.Result;
using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Specifications;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Curriculums.Features;

public class ListCurriculumsQuery : IQuery<Result<List<CurriculumDTO>>>
{
}

public class ListCurriculumsQueryHandler(IReadRepository<Curriculum> readRepository)
    : IQueryHandler<ListCurriculumsQuery, Result<List<CurriculumDTO>>>
{
    public async ValueTask<Result<List<CurriculumDTO>>> Handle(ListCurriculumsQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListCurriculumSpec();
        var curriculums = await readRepository.ListAsync(spec, cancellationToken);

        var toReturn =
            curriculums.Select(CurriculumDTO.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}