using System.Linq.Expressions;
using Ardalis.Specification;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Features.Subjects.Specifications;

public class FilterSubjectsPaginatedSpec : Specification<Subject>
{
    private static readonly HashSet<string> AllowedFilterColumns = [
        nameof(Subject.Code).ToLower(),
        nameof(Subject.Title).ToLower(),
        nameof(Subject.Description).ToLower(),
        nameof(Subject.Units).ToLower()
    ];

    // The explicit cast `(object?)s.Code` boxes the Vogen value object; EF Core still generates correct SQL.
    private static readonly IReadOnlyDictionary<string, Expression<Func<Subject, object?>>> SortMap =
        new Dictionary<string, Expression<Func<Subject, object?>>>
        {
            ["code"]        = s => (object?)s.Code,
            ["title"]       = s => s.Title,
            ["units"]       = s => (object?)s.Units,
            ["description"] = s => s.Description
        };

    private readonly SpecSortBuilder<Subject> _sortBuilder = new();

    public FilterSubjectsPaginatedSpec(int pageNumber, int pageSize, IEnumerable<FilterItem>? filters, IEnumerable<SortItem>? sorts, JoinOperator joinOperator = JoinOperator.and)
    {
        // The explicit cast `(string)s.Code` leverages Vogen's generated explicit operator to let EF Core resolve
        // it to the underlying string column. No EF Core dependency needed — only Ardalis.Specification.
        Query
            .AsNoTracking()
            .Include(s => s.PreferRoomType);

        var filterExpressions = new List<Expression<Func<Subject, bool>>>();

        foreach (var filter in filters ?? [])
        {
            if (!AllowedFilterColumns.Contains(filter.Id)) continue;

            var expr = filter.Id switch
            {
                "code"        => FilterExpressionBuilder.ForString<Subject>(s => (string)s.Code, filter),
                "title"       => FilterExpressionBuilder.ForString<Subject>(s => s.Title, filter),
                "description" => FilterExpressionBuilder.ForString<Subject>(s => s.Description, filter),
                "units"       => decimal.TryParse(filter.Value, out var unitsValue)
                                    ? FilterExpressionBuilder.ForNumeric<Subject, decimal>(s => s.Units, unitsValue, filter)
                                    : null,
                _ => null
            };

            if (expr is not null) filterExpressions.Add(expr);
        }

        var combined = FilterExpressionBuilder.Combine(filterExpressions, joinOperator);
        if (combined is not null) Query.Where(combined);

        var sortList = sorts?.ToList() ?? [];
        if (sortList.Count > 0)
        {
            _sortBuilder.Apply(Query, sortList[0], isFirst: true, SortMap);
            for (int i = 1; i < sortList.Count; i++)
                _sortBuilder.Apply(Query, sortList[i], isFirst: false, SortMap);
        }
        else
        {
            Query.OrderBy(s => s.Title);
        }

        Query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
