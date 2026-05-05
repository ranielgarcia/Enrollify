using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Queries;

public class GetActiveAcademicYearQuery : IQuery<Result<AcademicYearDto>>;

public sealed class GetActiveAcademicYearQueryHandler : IQueryHandler<GetActiveAcademicYearQuery, Result<AcademicYearDto>>
{
    private readonly IReadRepository<AcademicYear> _readRepository;

    public GetActiveAcademicYearQueryHandler(IReadRepository<AcademicYear> readRepository)
    {
        _readRepository = readRepository;
    }

    public async ValueTask<Result<AcademicYearDto>> Handle(GetActiveAcademicYearQuery query, CancellationToken cancellationToken)
    {
        var activeAcademicYear = await _readRepository.FirstOrDefaultAsync(new GetActiveAcademicYearSpec());
        if (activeAcademicYear is null)
        {
            return Result.NotFound("No active academic year found.");
        }
        return Result.Success(AcademicYearDto.FromEntity(activeAcademicYear));
    }
}
