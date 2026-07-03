using System.Linq.Expressions;

namespace Enrollify.Application.Filtering;

/// <summary>
/// Manages OrderBy/ThenBy chaining for Ardalis.Specification queries.
/// Eliminates the need for a manual _orderedQuery field and switch statements in each spec.
/// </summary>
public sealed class SpecSortBuilder<T> where T : class
{
    private IOrderedSpecificationBuilder<T>? _orderedQuery;

    /// <summary>
    /// Applies a sort item using a pre-defined sort map.
    /// Silently skips unknown sort IDs.
    /// </summary>
    public void Apply(
        ISpecificationBuilder<T> query,
        SortItem sort,
        bool isFirst,
        IReadOnlyDictionary<string, Expression<Func<T, object?>>> sortMap)
    {
        if (!sortMap.TryGetValue(sort.Id, out var selector))
            return;

        if (isFirst)
        {
            _orderedQuery = sort.Desc
                ? query.OrderByDescending(selector)
                : query.OrderBy(selector);
        }
        else if (_orderedQuery is not null)
        {
            _orderedQuery = sort.Desc
                ? _orderedQuery.ThenByDescending(selector)
                : _orderedQuery.ThenBy(selector);
        }
    }
}
