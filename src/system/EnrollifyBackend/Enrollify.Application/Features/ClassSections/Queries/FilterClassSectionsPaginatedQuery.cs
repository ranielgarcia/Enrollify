using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.DTOs;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSections.Queries;

public record FilterClassSectionsPaginatedQuery(
  int page = 1,
  int pageSize = 10,
  int? academicYearId = null,
  IEnumerable<FilterItem>? filters = null,
  IEnumerable<SortItem>? sorts = null,
  string? joinOperator = null
) : IRequest<Result<PagedResult<ClassSectionDto>>>;

public class FilterClassSectionsPaginatedQueryHandler
  : IRequestHandler<FilterClassSectionsPaginatedQuery, Result<PagedResult<ClassSectionDto>>>
{
  private readonly IReadRepository<ClassSection> _readRepository;
  private readonly IReadRepository<AcademicYear> _academicYearRepository;

  public FilterClassSectionsPaginatedQueryHandler(
    IReadRepository<ClassSection> readRepository,
    IReadRepository<AcademicYear> academicYearRepository)
  {
    _readRepository = readRepository;
    _academicYearRepository = academicYearRepository;
  }

  public async Task<Result<PagedResult<ClassSectionDto>>> Handle(
    FilterClassSectionsPaginatedQuery request,
    CancellationToken cancellationToken)
  {
    JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.joinOperator, true, out JoinOperator parsed)
      ? parsed
      : JoinOperator.Or;

    IEnumerable<int>? academicTermIds = null;
    if (request.academicYearId.HasValue)
    {
      var academicYearId = AcademicYearId.From(request.academicYearId.Value);
      AcademicYear? academicYear = await _academicYearRepository.FirstOrDefaultAsync(
        new GetAcademicYearByIdSpec(academicYearId), cancellationToken);

      if (academicYear is null)
        return Result.NotFound($"Academic year with id {request.academicYearId} was not found.");

      academicTermIds = academicYear.AcademicTerms.Select(t => t.Id.Value);
    }

    var spec = new FilterClassSectionsPaginatedSpec(
      request.page,
      request.pageSize,
      academicTermIds,
      request.filters,
      request.sorts,
      joinOperator);

    List<ClassSection> sections = await _readRepository.ListAsync(spec, cancellationToken);
    int totalCount = await _readRepository.CountAsync(spec, cancellationToken);

    var items = sections
      .Select(ClassSectionDto.FromEntity)
      .ToList();

    return new PagedResult<ClassSectionDto>(
      items.AsReadOnly(),
      request.page,
      request.pageSize,
      totalCount);
  }
}
