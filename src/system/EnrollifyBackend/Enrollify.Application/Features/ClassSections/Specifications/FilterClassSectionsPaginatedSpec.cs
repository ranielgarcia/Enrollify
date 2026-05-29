using System.Linq.Expressions;
using Ardalis.Specification;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class FilterClassSectionsPaginatedSpec : Specification<ClassSection>
{
  private static readonly HashSet<string> AllowedFilterColumns =
  [
    nameof(ClassSection.Name).ToLower(),
    "coursename",
    "coursecode",
    "intendedyearlevel",
    "curriculumversion",
    "advisername",
    "adviseremail",
    "sectioncode",
    "status"
  ];

  private static readonly IReadOnlyDictionary<string, Expression<Func<ClassSection, object?>>> SortMap =
    new Dictionary<string, Expression<Func<ClassSection, object?>>>
    {
      ["name"] = cs => cs.Name,
      ["coursename"] = cs => (object?)(cs.Course != null ? cs.Course.Name : null),
      ["intendedyearlevel"] = cs => (object?)(int)cs.IntendedYearLevel,
      ["sectioncode"] = cs => (object?)(char)cs.SectionCode,
      ["status"] = cs => (object?)cs.StatusId.Value,
      ["advisername"] = cs => (object?)(cs.Adviser != null ? cs.Adviser.LastName : null)
    };

  private readonly SpecSortBuilder<ClassSection> _sortBuilder = new();

  public FilterClassSectionsPaginatedSpec(
    int pageNumber,
    int pageSize,
    IEnumerable<int>? academicTermIds,
    IEnumerable<FilterItem>? filters,
    IEnumerable<SortItem>? sorts,
    JoinOperator joinOperator = JoinOperator.And)
  {
    Query
      .AsNoTracking()
      .Include(cs => cs.Course)
      .Include(cs => cs.Curriculum)
      .Include(cs => cs.Adviser)
      .Include(cs => cs.AcademicTerm)
      .Include(cs => cs.CohortAcademicYear)
      .Include(cs => cs.CreatedByUser)
      .Include(cs => cs.UpdatedByUser);

    // Mandatory scope: filter to the given academic year's terms
    List<int> termIdList = academicTermIds?.ToList() ?? [];
    if (termIdList.Count > 0)
    {
      var termIds = termIdList.Select(AcademicTermId.From).ToList();
      Query.Where(cs => termIds.Contains(cs.AcademicTermId));
    }

    var filterExpressions = new List<Expression<Func<ClassSection, bool>>>();

    foreach (FilterItem filter in filters ?? [])
    {
      string filterId = filter.Id.ToLower();
      if (!AllowedFilterColumns.Contains(filterId, StringComparer.InvariantCultureIgnoreCase)) continue;

      Expression<Func<ClassSection, bool>>? expr = filterId switch
      {
        "name" =>
          FilterExpressionBuilder.ForString<ClassSection>(cs => cs.Name, filter),

        "coursename" =>
          FilterExpressionBuilder.ForNullableString<ClassSection>(
            cs => cs.Course != null ? cs.Course.Name : null, filter),

        "coursecode" =>
          FilterExpressionBuilder.ForNullableString<ClassSection>(
            cs => cs.Course != null ? (string)cs.Course.Code : null, filter),

        "intendedyearlevel" when int.TryParse(filter.Value, out int parsedYearLevel) =>
          FilterExpressionBuilder.ForNumeric<ClassSection, int>(
            cs => (int)cs.IntendedYearLevel, parsedYearLevel, filter),

        "curriculumversion" =>
          FilterExpressionBuilder.ForNullableString<ClassSection>(
            cs => cs.Curriculum != null ? cs.Curriculum.Version : null, filter),

        "advisername" =>
          FilterExpressionBuilder.ForNullableString<ClassSection>(
            cs => cs.Adviser != null ? cs.Adviser.FirstName + " " + cs.Adviser.LastName : null, filter),

        "adviseremail" =>
          FilterExpressionBuilder.ForNullableString<ClassSection>(
            cs => cs.Adviser != null ? (string)cs.Adviser.Email : null, filter),

        "sectioncode" when filter.Value.Length == 1 =>
          FilterExpressionBuilder.ForChar<ClassSection>(
            cs => (char)cs.SectionCode, filter),

        "status" when int.TryParse(filter.Value, out int parsedStatus) =>
          FilterExpressionBuilder.ForNumeric<ClassSection, int>(
            cs => cs.StatusId.Value, parsedStatus, filter),

        _ => null
      };

      if (expr is not null) filterExpressions.Add(expr);
    }

    Expression<Func<ClassSection, bool>>? combined = FilterExpressionBuilder.Combine(filterExpressions, joinOperator);
    if (combined is not null) Query.Where(combined);

    List<SortItem> sortList = sorts?.ToList() ?? [];
    if (sortList.Count > 0)
    {
      _sortBuilder.Apply(Query, sortList[0], true, SortMap);
      for (int i = 1; i < sortList.Count; i++)
        _sortBuilder.Apply(Query, sortList[i], false, SortMap);
    }
    else
    {
      Query.OrderBy(cs => cs.Name).ThenBy(cs => (char)cs.SectionCode);
    }

    Query
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize);
  }
}
