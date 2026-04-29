# Filter Specification Helper Design

## Problem Statement

Current filter specification classes (`FilterSubjectsPaginatedSpec`, `FilterTeachersPaginatedSpec`, and future specs) contain significant code duplication:

### Duplicated Code Blocks (per spec)

| Pattern | Lines | Occurrences |
|---------|-------|-------------|
| **String property filter case** | ~8 lines | 1 per string field |
| **Nullable string? filter case** | ~8 lines | 1 per nullable field |
| **Vogen-cast string filter case** | ~8 lines | 1 per value-object field |
| **Numeric filter case** | ~8 lines | 1 per numeric field |
| **Filter aggregation block** | ~10 lines | 1 per spec |
| **Sort application method** | ~25 lines | 1 per spec |
| **ExpressionParameterReplacer class** | ~10 lines | 1 per spec (file-scoped duplicate) |

### Example: Subject vs Teacher

Both specs implement nearly identical logic for:
- String filtering with 6 operators (`ILike`, `NotILike`, `Eq`, `Ne`, `IsEmpty`, `IsNotEmpty`)
- Expression combining with `AND`/`OR` logic via `ExpressionParameterReplacer`
- Sort chaining with `OrderBy` → `ThenBy` pattern via `_orderedQuery` handle

**Current State:**
- `FilterSubjectsPaginatedSpec.cs`: ~170 lines
- `FilterTeachersPaginatedSpec.cs`: ~280 lines
- Expected future specs (Student, Room, Course, etc.): 200-300 lines each

**After Refactoring:**
- Estimated: 60-80 lines per spec
- **Savings: ~70% reduction** in boilerplate

---

## Proposed Solution

### Architecture Overview

Create two reusable components in the `Enrollify.Application.Filtering` namespace:

```
Enrollify.Application/
└── Filtering/
    ├── FilterItem.cs              (existing)
    ├── FilterOperator.cs          (existing)
    ├── SortItem.cs                (existing)
    ├── JoinOperator.cs            (existing)
    ├── FilterExpressionBuilder.cs (NEW)
    └── SpecSortBuilder.cs         (NEW)
```

#### 1. `FilterExpressionBuilder` (static class)
Factory methods that generate `Expression<Func<T, bool>>` from filter inputs.

#### 2. `SpecSortBuilder<T>` (instance class)
Manages the `_orderedQuery` handle and applies chained sorting.

#### 3. `ExpressionParameterReplacer`
Moves from file-scoped duplication → internal implementation in `FilterExpressionBuilder`.

---

## Detailed Design

### 1. FilterExpressionBuilder

```csharp
namespace Enrollify.Application.Filtering;

public static class FilterExpressionBuilder
{
    /// <summary>
    /// Build filter expression for non-nullable string properties.
    /// Also works for Vogen value objects when caller provides cast: t => ((string)t.Email)
    /// </summary>
    public static Expression<Func<T, bool>>? ForString<T>(
        Expression<Func<T, string>> selector,
        FilterItem filter)
    {
        return filter.Operator switch
        {
            FilterOperator.ILike        => Combine(selector, s => s.Contains(filter.Value)),
            FilterOperator.NotILike     => Combine(selector, s => !s.Contains(filter.Value)),
            FilterOperator.Eq           => Combine(selector, s => s == filter.Value),
            FilterOperator.Ne           => Combine(selector, s => s != filter.Value),
            FilterOperator.IsEmpty      => Combine(selector, s => s == string.Empty),
            FilterOperator.IsNotEmpty   => Combine(selector, s => s != string.Empty),
            _ => null
        };
    }

    /// <summary>
    /// Build filter expression for nullable string? properties.
    /// Handles null checks for ILike/NotILike operators.
    /// </summary>
    public static Expression<Func<T, bool>>? ForNullableString<T>(
        Expression<Func<T, string?>> selector,
        FilterItem filter)
    {
        return filter.Operator switch
        {
            FilterOperator.ILike        => Combine(selector, s => s != null && s.Contains(filter.Value)),
            FilterOperator.NotILike     => Combine(selector, s => s == null || !s.Contains(filter.Value)),
            FilterOperator.Eq           => Combine(selector, s => s == filter.Value),
            FilterOperator.Ne           => Combine(selector, s => s != filter.Value),
            FilterOperator.IsEmpty      => Combine(selector, s => string.IsNullOrEmpty(s)),
            FilterOperator.IsNotEmpty   => Combine(selector, s => !string.IsNullOrEmpty(s)),
            _ => null
        };
    }

    /// <summary>
    /// Build filter expression for numeric properties (decimal, int, etc.).
    /// Caller must pre-parse the value before calling this method.
    /// </summary>
    public static Expression<Func<T, bool>>? ForNumeric<T, TValue>(
        Expression<Func<T, TValue>> selector,
        TValue parsedValue,
        FilterItem filter)
        where TValue : struct, IComparable<TValue>
    {
        return filter.Operator switch
        {
            FilterOperator.Eq  => Combine(selector, v => v.CompareTo(parsedValue) == 0),
            FilterOperator.Ne  => Combine(selector, v => v.CompareTo(parsedValue) != 0),
            FilterOperator.Lt  => Combine(selector, v => v.CompareTo(parsedValue) < 0),
            FilterOperator.Lte => Combine(selector, v => v.CompareTo(parsedValue) <= 0),
            FilterOperator.Gt  => Combine(selector, v => v.CompareTo(parsedValue) > 0),
            FilterOperator.Gte => Combine(selector, v => v.CompareTo(parsedValue) >= 0),
            _ => null
        };
    }

    /// <summary>
    /// Combine multiple filter expressions with AND/OR logic.
    /// </summary>
    public static Expression<Func<T, bool>>? Combine<T>(
        IEnumerable<Expression<Func<T, bool>>> expressions,
        JoinOperator joinOperator)
    {
        var list = expressions.ToList();
        if (list.Count == 0) return null;
        
        return list.Aggregate((left, right) =>
        {
            var param = left.Parameters[0];
            var rightBody = ExpressionParameterReplacer.Replace(right.Body, right.Parameters[0], param);
            var body = joinOperator == JoinOperator.or
                ? Expression.OrElse(left.Body, rightBody)
                : Expression.AndAlso(left.Body, rightBody);
            return Expression.Lambda<Func<T, bool>>(body, param);
        });
    }

    // Internal helper to compose selector + condition
    private static Expression<Func<T, bool>> Combine<T, TProp>(
        Expression<Func<T, TProp>> selector,
        Expression<Func<TProp, bool>> condition)
    {
        var param = selector.Parameters[0];
        var conditionBody = ExpressionParameterReplacer.Replace(
            condition.Body,
            condition.Parameters[0],
            selector.Body);
        return Expression.Lambda<Func<T, bool>>(conditionBody, param);
    }

    // Internal expression rewriter (previously file-scoped in each spec)
    private sealed class ExpressionParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);

        public static Expression Replace(Expression body, ParameterExpression from, Expression to)
            => new ExpressionParameterReplacer(from, to).Visit(body);
    }
}


### 2. SpecSortBuilder

```csharp
namespace Enrollify.Application.Filtering;

using Ardalis.Specification;

/// <summary>
/// Manages OrderBy/ThenBy chaining for Ardalis.Specification queries.
/// Eliminates the need for manual _orderedQuery field + switch statements.
/// </summary>
public sealed class SpecSortBuilder<T> where T : class
{
    private IOrderedSpecificationBuilder<T>? _orderedQuery;

    /// <summary>
    /// Apply a sort item using a pre-defined sort map.
    /// </summary>
    /// <param name="query">The specification builder to sort</param>
    /// <param name="sort">Sort item from the request</param>
    /// <param name="isFirst">True for first sort (OrderBy), false for subsequent (ThenBy)</param>
    /// <param name="sortMap">Dictionary mapping sort IDs to selector expressions</param>
    public void Apply(
        ISpecificationBuilder<T> query,
        SortItem sort,
        bool isFirst,
        IReadOnlyDictionary<string, Expression<Func<T, object?>>> sortMap)
    {
        if (!sortMap.TryGetValue(sort.Id, out var selector))
        {
            // Unknown sort ID - skip
            return;
        }

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
```

### Usage Examples

#### Before (Current FilterTeachersPaginatedSpec)

```csharp
// 280 lines total
// Filter switch - repeated for each field
case "firstname":
    Expression<Func<Teacher, bool>>? firstNameExpr = filter.Operator switch
    {
        FilterOperator.ILike => t => t.FirstName.Contains(filter.Value),
        FilterOperator.NotILike => t => !t.FirstName.Contains(filter.Value),
        FilterOperator.Eq => t => t.FirstName == filter.Value,
        FilterOperator.Ne => t => t.FirstName != filter.Value,
        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.FirstName),
        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.FirstName),
        _ => null
    };
    if (firstNameExpr is not null) filterExpressions.Add(firstNameExpr);
    break;

// ... repeated 12 more times for other fields ...

// Combine filters
if (filterExpressions.Count > 0)
{
    var combined = filterExpressions.Aggregate((left, right) =>
    {
        var param = left.Parameters[0];
        var rightBody = ExpressionParameterReplacer.Replace(right.Body, right.Parameters[0], param);
        var body = joinOperator == JoinOperator.or
            ? Expression.OrElse(left.Body, rightBody)
            : Expression.AndAlso(left.Body, rightBody);
        return Expression.Lambda<Func<Teacher, bool>>(body, param);
    });
    Query.Where(combined);
}

// Sort application - 25 lines
private IOrderedSpecificationBuilder<Teacher>? _orderedQuery;
private void ApplySort(SortItem sort, bool isFirst)
{
    if (isFirst)
    {
        _orderedQuery = sort.Id switch
        {
            "firstname" => sort.Desc ? Query.OrderByDescending(t => t.FirstName) : Query.OrderBy(t => t.FirstName),
            // ... 8 more cases ...
            _ => null
        };
    }
    else if (_orderedQuery is not null)
    {
        _orderedQuery = sort.Id switch
        {
            "firstname" => sort.Desc ? _orderedQuery.ThenByDescending(t => t.FirstName) : _orderedQuery.ThenBy(t => t.FirstName),
            // ... 8 more cases ...
            _ => _orderedQuery
        };
    }
}

// File-scoped duplicate
file sealed class ExpressionParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
{
    // ...10 lines...
}
After (Refactored FilterTeachersPaginatedSpec)
// Estimated ~80-100 lines total

using Enrollify.Application.Filtering;

public class FilterTeachersPaginatedSpec : Specification<Teacher>
{
    private static readonly HashSet<string> AllowedFilterColumns = [
        nameof(Teacher.FirstName).ToLower(),
        // ... other fields ...
    ];

    private static readonly Dictionary<string, Expression<Func<Teacher, object?>>> SortMap = new()
    {
        ["firstname"]        = t => t.FirstName,
        ["lastname"]         = t => t.LastName,
        ["teacheridentifier"]= t => t.TeacherIdentifier,
        ["email"]            = t => t.Email,
        ["academictitle"]    = t => (object?)t.AcademicTitle,
        // ... 4 more mappings ...
    };

    private readonly SpecSortBuilder<Teacher> _sortBuilder = new();

    public FilterTeachersPaginatedSpec(
        int pageNumber,
        int pageSize,
        IEnumerable<FilterItem>? filters,
        IEnumerable<SortItem>? sorts,
        JoinOperator joinOperator = JoinOperator.and)
    {
        Query
            .AsNoTracking()
            .Include(t => t.Department);

        var filterExpressions = new List<Expression<Func<Teacher, bool>>>();

        foreach (var filter in filters ?? [])
        {
            if (!AllowedFilterColumns.Contains(filter.Id)) continue;

            var expr = filter.Id switch
            {
                // Non-nullable strings
                "firstname"  => FilterExpressionBuilder.ForString<Teacher>(t => t.FirstName, filter),
                "middlename" => FilterExpressionBuilder.ForString<Teacher>(t => t.MiddleName, filter),
                "lastname"   => FilterExpressionBuilder.ForString<Teacher>(t => t.LastName, filter),

                // Vogen value objects (explicit cast)
                "teacheridentifier" => FilterExpressionBuilder.ForString<Teacher>(t => (string)t.TeacherIdentifier, filter),
                "email"             => FilterExpressionBuilder.ForString<Teacher>(t => (string)t.Email, filter),
                "phonenumber"       => FilterExpressionBuilder.ForString<Teacher>(t => (string)t.PhoneNumber, filter),

                // Nullable strings
                "academictitle"  => FilterExpressionBuilder.ForNullableString<Teacher>(t => t.AcademicTitle, filter),
                "qualification"  => FilterExpressionBuilder.ForNullableString<Teacher>(t => t.Qualification, filter),
                "specialization" => FilterExpressionBuilder.ForNullableString<Teacher>(t => t.Specialization, filter),
                "officelocation" => FilterExpressionBuilder.ForNullableString<Teacher>(t => t.OfficeLocation, filter),
                "officehours"    => FilterExpressionBuilder.ForNullableString<Teacher>(t => t.OfficeHours, filter),
                "biography"      => FilterExpressionBuilder.ForNullableString<Teacher>(t => t.Biography, filter),

                _ => null
            };

            if (expr is not null) filterExpressions.Add(expr);
        }

        // Combine filters - 1 line
        var combined = FilterExpressionBuilder.Combine(filterExpressions, joinOperator);
        if (combined is not null) Query.Where(combined);

        // Apply sorts
        var sortList = sorts?.ToList() ?? [];
        if (sortList.Count > 0)
        {
            _sortBuilder.Apply(Query, sortList[0], isFirst: true, SortMap);
            for (int i = 1; i < sortList.Count; i++)
                _sortBuilder.Apply(Query, sortList[i], isFirst: false, SortMap);
        }
        else
        {
            Query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);
        }

        Query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}

```

**Line count comparison:**
- Before: ~280 lines (Teacher spec)
- After: ~100 lines
- Reduction: ~64%

---

## Benefits

### 1. DRY Principle
- `ExpressionParameterReplacer` defined once (not 10+ times)
- Filter operator logic centralized (6 operators × 4 types = 24 switch arms → 4 methods)
- Sort chaining logic reused across all specs

### 2. Maintainability
- Adding new operator (e.g., `StartsWith`) requires change in 1 place instead of every spec
- Bug fixes propagate to all specs automatically
- Consistent behavior across all entities

### 3. Readability
- Spec files become declarative (what to filter/sort) not imperative (how to build expressions)
- Switch statement → concise mapping
- Intent clearer: `ForString(t => t.FirstName, filter)` vs 8-line switch block

### 4. Type Safety
- Generic methods ensure compile-time checks
- `ForNumeric<T, TValue>` enforces `IComparable<TValue>` constraint
- Sort map types verified at declaration

---

## Implementation Notes

### Numeric Filtering

Caller must parse values before passing to `ForNumeric`:

```csharp
case "units":
    if (!decimal.TryParse(filter.Value, out var unitsValue)) break;
    var expr = FilterExpressionBuilder.ForNumeric<Subject, decimal>(
        s => s.Units,
        unitsValue,
        filter);
    if (expr is not null) filterExpressions.Add(expr);
    break;
```

**Rationale:** Keeps `FilterExpressionBuilder` free of parsing logic and domain knowledge.

### Sort Map Boxing

`SpecSortBuilder` uses `Expression<Func<T, object?>>` for generic sorting. This causes boxing for value types (e.g., `decimal Units`).

**Alternatives:**
1. Accept boxing — simplicity vs minor performance cost (EF Core still generates correct SQL)
2. Keep switch statements for sorting — performance-critical scenarios
3. Dual maps — `Dictionary<string, Func<ISpecificationBuilder<T>, IOrderedSpecificationBuilder<T>>>` for unboxed lambdas (verbose)

**Recommendation:** Start with Option 1 (boxing). Profile if performance becomes an issue.

### Vogen Value Objects

Explicit casts required for Vogen types:

```csharp
// SubjectCode is Vogen type with explicit string operator
"code" => FilterExpressionBuilder.ForString<Subject>(s => (string)s.Code, filter)
```

EF Core sees the cast and translates to SQL column correctly.

---

## Open Questions

### 1. Should `ForNumeric` accept `string` and parse internally?

**Current:**

```csharp
if (!decimal.TryParse(filter.Value, out var units)) break;
var expr = FilterExpressionBuilder.ForNumeric(s => s.Units, units, filter);
```

**Alternative:**

```csharp
var expr = FilterExpressionBuilder.ForNumeric(s => s.Units, filter);
// Internally: decimal.TryParse(filter.Value, ...)
```

**Decision:** Keep parsing in caller — spec knows the type (`decimal` vs `int` vs `double`).

### 2. Should sort map handle unknown IDs differently?

**Current:** `SpecSortBuilder.Apply` silently skips unknown sort IDs.

**Alternative:** Throw exception or log warning.

**Decision:** Skip silently (client may send stale sort IDs after schema change).

### 3. Support for complex filters (date ranges, array contains)?

**Current:** Only string/numeric operators supported.

**Future:** Add methods like:

```csharp
ForDate<T>(Expression<Func<T, DateTime>> selector, FilterItem filter)
ForArray<T>(Expression<Func<T, IEnumerable<string>>> selector, FilterItem filter)
```

**Decision:** Implement when needed (YAGNI principle).

---

## Migration Strategy

### Phase 1 — Implement Core Classes
1. Create `FilterExpressionBuilder.cs` with `ForString`, `ForNullableString`, `ForNumeric`, `Combine`
2. Create `SpecSortBuilder.cs` with `Apply` method
3. Write unit tests for both classes

### Phase 2 — Refactor Existing Specs
1. Refactor `FilterSubjectsPaginatedSpec` (simpler — only 4 fields)
2. Refactor `FilterTeachersPaginatedSpec` (12 fields)
3. Verify existing integration tests pass
4. Compare SQL queries generated before/after

### Phase 3 — Apply to New Specs
- Use refactored pattern for all future specs (Student, Room, Course, etc.)

---

## File Locations

```
D:\Enrollment-System\
├── analysis\
│   └── filter-spec-helper-design.md          (this file)
└── src\system\EnrollifyBackend\
    └── Enrollify.Application\
        ├── Features\
        │   ├── Subjects\Specifications\
        │   │   └── FilterSubjectsPaginatedSpec.cs    (refactor)
        │   └── Teachers\Specifications\
        │       └── FilterTeachersPaginatedSpec.cs    (refactor)
        └── Filtering\
            ├── FilterItem.cs                  (existing)
            ├── FilterOperator.cs              (existing)
            ├── SortItem.cs                    (existing)
            ├── JoinOperator.cs                (existing)
            ├── FilterExpressionBuilder.cs     (NEW)
            └── SpecSortBuilder.cs             (NEW)
```

---

## Success Criteria

- [ ] `FilterExpressionBuilder` and `SpecSortBuilder` implemented
- [ ] Unit tests cover all filter operators and sort scenarios
- [ ] `FilterSubjectsPaginatedSpec` refactored (<80 lines)
- [ ] `FilterTeachersPaginatedSpec` refactored (<120 lines)
- [ ] All existing tests pass without modification
- [ ] Generated SQL queries identical to pre-refactor
- [ ] Documentation updated with usage examples

---

## Estimated Impact

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Lines per spec (avg) | ~250 | ~90 | 64% reduction |
| Duplicate code blocks | 10+ per spec | 0 | 100% elimination |
| New spec creation time | ~30 min | ~10 min | 67% faster |
| Maintenance burden | High (N specs × M operators) | Low (1 helper class) | 10x easier |

---

## Conclusion

The proposed `FilterExpressionBuilder` and `SpecSortBuilder` classes eliminate 60-70% of boilerplate in filter specifications while maintaining:
- Type safety (compile-time checks)
- Performance (same EF Core SQL generation)
- Flexibility (easy to extend with new operators)
- Clarity (declarative spec definitions)
This refactoring will pay immediate dividends and scale elegantly as the system grows to support additional entities (Student, Room, Course, Section, Schedule, etc.).