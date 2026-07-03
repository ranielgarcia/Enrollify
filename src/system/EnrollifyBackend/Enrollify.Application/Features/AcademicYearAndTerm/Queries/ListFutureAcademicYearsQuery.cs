using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Queries;

public class ListFutureAcademicYearsQuery : IRequest<Result<List<AcademicYearDto>>> { }

public sealed class ListFutureAcademicYearsQueryHandler : IRequestHandler<ListFutureAcademicYearsQuery, Result<List<AcademicYearDto>>>
{
    private readonly IReadRepository<AcademicYear> _repository;

    public ListFutureAcademicYearsQueryHandler(IReadRepository<AcademicYear> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AcademicYearDto>>> Handle(ListFutureAcademicYearsQuery query, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var spec = new ListFutureAcademicYearsSpec(today);
        var previousAcademicYears = await _repository.ListAsync(spec, cancellationToken);
        var previousAcademicYearDtos = previousAcademicYears.Select(AcademicYearDto.FromEntity).ToList();
        return Result.Success(previousAcademicYearDtos);
    }
}
