using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Application.Features.Curriculums.DTOs;

namespace Enrollify.Application.Features.Curriculums.Queries;

public class ListCurriculumsQuery : IRequest<Result<List<CurriculumDto>>>
{
}

public class ListCurriculumsQueryHandler(IReadRepository<Curriculum> readRepository)
    : IRequestHandler<ListCurriculumsQuery, Result<List<CurriculumDto>>>
{
    public async Task<Result<List<CurriculumDto>>> Handle(ListCurriculumsQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListCurriculumsSpec();
        var curriculums = await readRepository.ListAsync(spec, cancellationToken);

        var toReturn =
            curriculums.Select(CurriculumDto.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
