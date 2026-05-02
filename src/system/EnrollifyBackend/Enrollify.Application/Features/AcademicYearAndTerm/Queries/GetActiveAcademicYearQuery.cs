using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Mediator;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Queries;

public class GetActiveAcademicYearQuery : IQuery<Result<AcademicYearDto>>;

public sealed class GetActiveAcademicYearQueryHandler : IQueryHandler<GetActiveAcademicYearQuery, Result<AcademicYearDto>>
{
    private readonly IAcademicYearAndTermRepository _repository;

    public GetActiveAcademicYearQueryHandler(IAcademicYearAndTermRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<AcademicYearDto>> Handle(GetActiveAcademicYearQuery query, CancellationToken cancellationToken)
    {
        var activeAcademicYear = await _repository.GetActiveAcademicYearAsync(cancellationToken);
        if (activeAcademicYear is null)
        {
            return Result.NotFound("No active academic year found.");
        }
        return Result.Success(AcademicYearDto.FromEntity(activeAcademicYear));
    }
}
