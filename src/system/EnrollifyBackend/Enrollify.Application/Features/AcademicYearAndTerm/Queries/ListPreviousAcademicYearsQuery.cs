using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Queries;

public class ListPreviousAcademicYearsQuery : IRequest<Result<List<AcademicYearDto>>>{}

public sealed class ListPreviousAcademicYearsQueryHandler : IRequestHandler<ListPreviousAcademicYearsQuery, Result<List<AcademicYearDto>>>
{
    private readonly IReadRepository<AcademicYear> _repository;

    public ListPreviousAcademicYearsQueryHandler(IReadRepository<AcademicYear> repository)
    {
        _repository = repository;
    }
    public async Task<Result<List<AcademicYearDto>>> Handle(ListPreviousAcademicYearsQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListPreviousAcademicYearsSpec(numberOfPreviousYears: 5);
        var previousAcademicYears = await _repository.ListAsync(spec, cancellationToken);

        var previousAcademicYearDtos = previousAcademicYears.Select(AcademicYearDto.FromEntity).ToList();
        return Result.Success(previousAcademicYearDtos);
    }
}
