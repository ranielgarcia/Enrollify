using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Queries;

public class GetAcademicYearTimelineWindowQuery : IQuery<Result<AcademicYearContextDto>>
{
    public bool IncludePastYears { get; set; }
    public int NumberOfPastYears { get; set; }
    public bool IncludeFutureYears { get; set; }
    public int NumberOfFutureYears { get; set; }
}

public sealed class GetAcademicYearTimelineWindowQueryHandler(IReadRepository<AcademicYear> repository)
    : IQueryHandler<GetAcademicYearTimelineWindowQuery, Result<AcademicYearContextDto>>
{
    public async ValueTask<Result<AcademicYearContextDto>> Handle(
        GetAcademicYearTimelineWindowQuery query, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var toReturn = new AcademicYearContextDto();

        if (query.IncludePastYears)
        {
            toReturn.Previous = (await repository.ListAsync(
                new ListPreviousAcademicYearsSpec(query.NumberOfPastYears), cancellationToken))
                .Select(AcademicYearDto.FromEntity).ToList();
        }

        if (query.IncludeFutureYears)
        {
            toReturn.Future = (await repository.ListAsync(
                new ListFutureAcademicYearsSpec(today, query.NumberOfFutureYears), cancellationToken))
                .Select(AcademicYearDto.FromEntity).ToList();
        }

        var current = await repository.FirstOrDefaultAsync(new GetActiveAcademicYearSpec(), cancellationToken);
        toReturn.Current = current is { } ay ? AcademicYearDto.FromEntity(ay) : null;

        return Result.Success(toReturn);
    }
}
