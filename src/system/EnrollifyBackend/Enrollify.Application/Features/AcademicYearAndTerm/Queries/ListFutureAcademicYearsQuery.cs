using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Queries;

public class ListFutureAcademicYearsQuery : IQuery<Result<List<AcademicYearDto>>> { }

public sealed class ListFutureAcademicYearsQueryHandler : IQueryHandler<ListFutureAcademicYearsQuery, Result<List<AcademicYearDto>>>
{
    private readonly IReadRepository<AcademicYear> _repository;

    public ListFutureAcademicYearsQueryHandler(IReadRepository<AcademicYear> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<List<AcademicYearDto>>> Handle(ListFutureAcademicYearsQuery query, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var spec = new ListFutureAcademicYearsSpec(today);
        var previousAcademicYears = await _repository.ListAsync(spec, cancellationToken);
        var previousAcademicYearDtos = previousAcademicYears.Select(AcademicYearDto.FromEntity).ToList();
        return Result.Success(previousAcademicYearDtos);
    }
}
