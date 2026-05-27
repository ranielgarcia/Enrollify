using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Queries;

public class GetCurrentAcademicYearQuery : IRequest<Result<AcademicYearDto>>;

public sealed class GetCurrentAcademicYearQueryHandler : IRequestHandler<GetCurrentAcademicYearQuery, Result<AcademicYearDto>>
{
    private readonly IReadRepository<AcademicYear> _readRepository;

    public GetCurrentAcademicYearQueryHandler(IReadRepository<AcademicYear> readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<AcademicYearDto>> Handle(GetCurrentAcademicYearQuery query, CancellationToken cancellationToken)
    {
        var activeAcademicYear = await _readRepository.FirstOrDefaultAsync(new GetActiveAcademicYearSpec(), cancellationToken);
        if (activeAcademicYear is null)
        {
            return Result.NotFound("No active academic year found.");
        }
        return Result.Success(AcademicYearDto.FromEntity(activeAcademicYear));
    }
}
