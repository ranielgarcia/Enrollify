using Ardalis.Result;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Application.Features.Curriculums.DTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Curriculums.Queries;

public class ListCurriculumsQuery : IQuery<Result<List<CurriculumDto>>>
{
}

public class ListCurriculumsQueryHandler(IReadRepository<Curriculum> readRepository)
    : IQueryHandler<ListCurriculumsQuery, Result<List<CurriculumDto>>>
{
    public async ValueTask<Result<List<CurriculumDto>>> Handle(ListCurriculumsQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListCurriculumsSpec();
        var curriculums = await readRepository.ListAsync(spec, cancellationToken);

        var toReturn =
            curriculums.Select(CurriculumDto.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
