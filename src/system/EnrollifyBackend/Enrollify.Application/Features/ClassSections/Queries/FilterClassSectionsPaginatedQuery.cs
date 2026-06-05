using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.DTOs;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Models;
using Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSections.Queries;

public record FilterClassSectionsPaginatedQuery(
  int Page = 1,
  int PageSize = 10,
  int? AcademicYearId = null,
  IEnumerable<FilterItem>? Filters = null,
  IEnumerable<SortItem>? Sorts = null,
  string? JoinOperator = null
) : IRequest<Result<PagedResult<ClassSectionDto>>>;

public class FilterClassSectionsPaginatedQueryHandler
  : IRequestHandler<FilterClassSectionsPaginatedQuery, Result<PagedResult<ClassSectionDto>>>
{
  private readonly IReadRepository<ClassSection> _readRepository;
  private readonly IReadRepository<AcademicYear> _academicYearRepository;
  private readonly IReadRepository<ClassSectionEnrollmentEligibilityValidationMessage> _validationMessageRepository;

  public FilterClassSectionsPaginatedQueryHandler(
    IReadRepository<ClassSection> readRepository,
    IReadRepository<AcademicYear> academicYearRepository,
    IReadRepository<ClassSectionEnrollmentEligibilityValidationMessage> validationMessageRepository)
  {
    _readRepository = readRepository;
    _academicYearRepository = academicYearRepository;
    _validationMessageRepository = validationMessageRepository;
  }

  public async Task<Result<PagedResult<ClassSectionDto>>> Handle(
    FilterClassSectionsPaginatedQuery request,
    CancellationToken cancellationToken)
  {
    JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.JoinOperator, true, out JoinOperator parsed)
      ? parsed
      : JoinOperator.Or;

    IEnumerable<int>? academicTermIds = null;
    if (request.AcademicYearId.HasValue)
    {
      var academicYearId = AcademicYearId.From(request.AcademicYearId.Value);
      AcademicYear? academicYear = await _academicYearRepository.FirstOrDefaultAsync(
        new GetAcademicYearByIdSpec(academicYearId), cancellationToken);

      if (academicYear is null)
        return Result.NotFound($"Academic year with id {request.AcademicYearId} was not found.");

      academicTermIds = academicYear.AcademicTerms.Select(t => t.Id.Value);
    }

    var spec = new FilterClassSectionsPaginatedSpec(
      request.Page,
      request.PageSize,
      academicTermIds,
      request.Filters,
      request.Sorts,
      joinOperator);

    List<ClassSection> sections = await _readRepository.ListAsync(spec, cancellationToken);
    int totalCount = await _readRepository.CountAsync(spec, cancellationToken);

    var sectionIds = sections.Select(s => s.Id).Distinct().ToList();

    List<ClassSectionEnrollmentEligibilityValidationMessage> validationMessagesPerSection =
      await _validationMessageRepository
        .ListAsync(new GetClassSectionEnrollmentEligibilityValidationErrorsSpec(sectionIds), cancellationToken);

    var validationMessagesPerSectionLookup = validationMessagesPerSection.GroupBy(m => m.ClassSectionId)
      .ToDictionary(g => g.Key, g => g.ToList());

    var sectionsToReturn = new List<ClassSectionDto>();
    foreach (ClassSection section in sections)
    {
      List<ClassSectionEnrollmentEligibilityValidationMessage> allValidationMessagesForCurrentSection =
        validationMessagesPerSectionLookup.ContainsKey(section.Id)
          ? validationMessagesPerSectionLookup[section.Id]
          : new List<ClassSectionEnrollmentEligibilityValidationMessage>();

      var sectionDto = ClassSectionDto.FromEntity(section, allValidationMessagesForCurrentSection.Count);
      sectionsToReturn.Add(sectionDto);
    }

    return new PagedResult<ClassSectionDto>(
      sectionsToReturn.AsReadOnly(),
      request.Page,
      request.PageSize,
      totalCount);
  }
}
