using Ardalis.Result;
using Enrollify.Application.Features.Curriculums.DTOs;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Models.Views;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Curriculums.Queries;

public class ListLatestActiveCurriculumsQuery : IRequest<Result<List<CurriculumDto>>>
{
}

public class ListLatestActiveCurriculumsQueryHandler
    (ICurriculumRepository curriculumRepository, IReadRepository<Curriculum> readRepository)
    : IRequestHandler<ListLatestActiveCurriculumsQuery, Result<List<CurriculumDto>>>
{
    public async Task<Result<List<CurriculumDto>>> Handle(ListLatestActiveCurriculumsQuery query, CancellationToken cancellationToken)
    {
        var result = await curriculumRepository.GetAllLatestActiveCurriculumPerCourse(cancellationToken);

        var curriculumIds = result.Select(c => c.CurriculumId).ToList();

        var curriculums = await readRepository.ListAsync(new GetCurriculumsWithSubjectsByIdsSpec(curriculumIds), cancellationToken);

        var toReturn =
            curriculums.Select(CurriculumDto.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
