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

    public FilterSubjectsPaginatedSpec(int pageNumber, int pageSize, IEnumerable<FilterItem>? filters, IEnumerable<SortItem>? sorts, JoinOperator joinOperator = JoinOperator.and)
    {
        // The explicit cast `((string)s.Code).Contains(searchTerm)` leverages Vogen's generated explicit operator string(SubjectCode) to let EF Core resolve it to the
        // underlying string column. string.Contains then translates to SQL LIKE '%term%'.
        // No EF Core dependency needed — only Ardalis.Specification.
        Query
            .AsNoTracking()
            .Include(s => s.PreferRoomType);

        var filterExpressions = new List<Expression<Func<Subject, bool>>>();

        foreach (var filter in filters ?? [])
        {
            if (!AllowedFilterColumns.Contains(filter.Id)) continue;

            switch (filter.Id)
            {
                case "code":

                    Expression<Func<Subject, bool>>? codeExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => s => ((string)s.Code).Contains(filter.Value),
                        FilterOperator.NotILike => s => !((string)s.Code).Contains(filter.Value),
                        FilterOperator.Eq => s => ((string)s.Code) == filter.Value,
                        FilterOperator.Ne => s => ((string)s.Code) != filter.Value,
                        FilterOperator.IsEmpty => s => ((string)s.Code) == string.Empty,
                        FilterOperator.IsNotEmpty => s => ((string)s.Code) != string.Empty,
                        _ => null
                    };
                    if (codeExpr is not null) filterExpressions.Add(codeExpr);
                    break;
                case "title":
                    Expression<Func<Subject, bool>>? titleExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => s => s.Title.Contains(filter.Value),
                        FilterOperator.NotILike => s => !s.Title.Contains(filter.Value),
                        FilterOperator.Eq => s => s.Title == filter.Value,
                        FilterOperator.Ne => s => s.Title != filter.Value,
                        FilterOperator.IsEmpty => s => string.IsNullOrEmpty(s.Title),
                        FilterOperator.IsNotEmpty => s => !string.IsNullOrEmpty(s.Title),
                        _ => null
                    };
                    if (titleExpr is not null) filterExpressions.Add(titleExpr);
                    break;
                case "description":
                    Expression<Func<Subject, bool>>? descExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => s => s.Description.Contains(filter.Value),
                        FilterOperator.NotILike => s => !s.Description.Contains(filter.Value),
                        FilterOperator.Eq => s => s.Description == filter.Value,
                        FilterOperator.Ne => s => s.Description != filter.Value,
                        FilterOperator.IsEmpty => s => string.IsNullOrEmpty(s.Description),
                        FilterOperator.IsNotEmpty => s => !string.IsNullOrEmpty(s.Description),
                        _ => null
                    };
                    if (descExpr is not null) filterExpressions.Add(descExpr);
                    break;
                case "units":
                    if (!decimal.TryParse(filter.Value, out var unitsValue)) break;
                    Expression<Func<Subject, bool>>? unitsExpr = filter.Operator switch
                    {
                        FilterOperator.Eq => s => s.Units == unitsValue,
                        FilterOperator.Ne => s => s.Units != unitsValue,
                        FilterOperator.Lt => s => s.Units < unitsValue,
                        FilterOperator.Lte => s => s.Units <= unitsValue,
                        FilterOperator.Gt => s => s.Units > unitsValue,
                        FilterOperator.Gte => s => s.Units >= unitsValue,
                        _ => null
                    };
                    if (unitsExpr is not null) filterExpressions.Add(unitsExpr);
                    break;
            }
        }

        if (filterExpressions.Count > 0)
        {
            var combined = filterExpressions.Aggregate((left, right) =>
            {
                var param = left.Parameters[0];
                var rightBody = ExpressionParameterReplacer.Replace(right.Body, right.Parameters[0], param);
                var body = joinOperator == JoinOperator.or
                    ? Expression.OrElse(left.Body, rightBody)
                    : Expression.AndAlso(left.Body, rightBody);
                return Expression.Lambda<Func<Subject, bool>>(body, param);
            });
            Query.Where(combined);
        }


        var sortList = sorts?.ToList() ?? [];
        if (sortList.Count > 0)
        {
            // Apply first sort
            ApplySort(sortList[0], isFirst: true);
            for (int i = 1; i < sortList.Count; i++)
                ApplySort(sortList[i], isFirst: false);
        }
        else
        {
            Query.OrderBy(s => s.Title);
        }

        Query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    private IOrderedSpecificationBuilder<Subject>? _orderedQuery;

    private void ApplySort(SortItem sort, bool isFirst)
    {
        // Query.OrderBy(...) / Query.OrderByDescending(...) in Ardalis.Specification return an IOrderedSpecificationBuilder<T>,
        // which is a separate, chainable builder object. It wraps the same underlying SpecificationBuilder internally,
        // so calling methods on it does mutate the same shared state — but only as long as you chain from it.
        //Query.OrderBy(...) internally calls Query.Add(...) on the specification's order expressions list and returns the
        //IOrderedSpecificationBuilder wrapper. The sort expression is already registered on Query's internal state at
        //that point._orderedQuery is just a handle you hold onto so you can call .ThenBy() / .ThenByDescending() on the
        //subsequent iterations.
        //In short: _orderedQuery is just a fluent chaining handle.
        //The order expressions are written directly into Query's internal specification state as each OrderBy/ThenBy call is made.
        //Your implementation is correct.
        if (isFirst)
        {
            _orderedQuery = sort.Id switch
            {
                "code"        => sort.Desc ? Query.OrderByDescending(s => s.Code)        : Query.OrderBy(s => s.Code),
                "title"       => sort.Desc ? Query.OrderByDescending(s => s.Title)       : Query.OrderBy(s => s.Title),
                "units"       => sort.Desc ? Query.OrderByDescending(s => s.Units)       : Query.OrderBy(s => s.Units),
                "description" => sort.Desc ? Query.OrderByDescending(s => s.Description) : Query.OrderBy(s => s.Description),
                _             => null
            };
        }
        else if (_orderedQuery is not null)
        {
            _orderedQuery = sort.Id switch
            {
                "code"        => sort.Desc ? _orderedQuery.ThenByDescending(s => s.Code)        : _orderedQuery.ThenBy(s => s.Code),
                "title"       => sort.Desc ? _orderedQuery.ThenByDescending(s => s.Title)       : _orderedQuery.ThenBy(s => s.Title),
                "units"       => sort.Desc ? _orderedQuery.ThenByDescending(s => s.Units)       : _orderedQuery.ThenBy(s => s.Units),
                "description" => sort.Desc ? _orderedQuery.ThenByDescending(s => s.Description) : _orderedQuery.ThenBy(s => s.Description),
                _             => _orderedQuery
            };
        }
    }
}

file sealed class ExpressionParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
        => node == from ? to : base.VisitParameter(node);

    public static Expression Replace(Expression body, ParameterExpression from, Expression to)
        => new ExpressionParameterReplacer(from, to).Visit(body);
}
