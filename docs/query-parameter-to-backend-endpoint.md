# Passing DiceUI Filter & Sort Query Parameters to the Backend

## Overview

The `ExtendedColumnFilter` format used by DiceUI's `DataTableFilterList` is **DiceUI-specific** — not a standard like OData or RSQL. There is no .NET library that understands it out of the box.

---

## Options

### Option 1: `System.Linq.Dynamic.Core` (Recommended)

Accept the filters as a JSON query param, deserialize to C# records, then map the DiceUI operators to LINQ/EF Core dynamically.

**Install:**
```
dotnet add package System.Linq.Dynamic.Core
```

**Create C# mirror types:**

```csharp
// FilterItem.cs
public record FilterItem
{
    public required string Id { get; init; }      // column name
    public required string Value { get; init; }   // filter value (or JSON array for inArray)
    public required string Variant { get; init; } // "text", "number", "select", etc.
    public required string Operator { get; init; }// "iLike", "eq", "lt", etc.
    public required string FilterId { get; init; }
}

public record SortItem
{
    public required string Id { get; init; }
    public required bool Desc { get; init; }
}
```

**Mapper to LINQ:**

```csharp
public static class FilterMapper
{
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        IEnumerable<FilterItem>? filters)
    {
        if (filters is null) return query;

        foreach (var filter in filters)
        {
            var column = filter.Id; // e.g. "code", "title"

            query = filter.Operator switch
            {
                "iLike"        => query.Where($"{column}.ToLower().Contains(@0)", filter.Value.ToLower()),
                "notILike"     => query.Where($"!{column}.ToLower().Contains(@0)", filter.Value.ToLower()),
                "eq"           => query.Where($"{column} == @0", filter.Value),
                "ne"           => query.Where($"{column} != @0", filter.Value),
                "lt"           => query.Where($"{column} < @0", filter.Value),
                "lte"          => query.Where($"{column} <= @0", filter.Value),
                "gt"           => query.Where($"{column} > @0", filter.Value),
                "gte"          => query.Where($"{column} >= @0", filter.Value),
                "isEmpty"      => query.Where($"{column} == null || {column} == \"\""),
                "isNotEmpty"   => query.Where($"{column} != null && {column} != \"\""),
                // inArray / notInArray need the value deserialized as string[]
                _ => query
            };
        }

        return query;
    }

    public static IQueryable<T> ApplySorts<T>(
        this IQueryable<T> query,
        IEnumerable<SortItem>? sorts)
    {
        if (sorts is null) return query;

        var sortList = sorts.ToList();
        for (int i = 0; i < sortList.Count; i++)
        {
            var direction = sortList[i].Desc ? "descending" : "ascending";
            query = i == 0
                ? query.OrderBy($"{sortList[i].Id} {direction}")
                : ((IOrderedQueryable<T>)query).ThenBy($"{sortList[i].Id} {direction}");
        }

        return query;
    }
}
```

**Update the FastEndpoints request + handler:**

```csharp
public class ListPaginatedSubjectsRequest
{
    [FromRoute] public required int Page { get; set; }
    [FromRoute] public required int PageSize { get; set; }

    [QueryParam] public string? Filters { get; set; }  // JSON string
    [QueryParam] public string? Sort { get; set; }     // JSON string
}
```

Then in the query handler:
```csharp
var filters = request.Filters is not null
    ? JsonSerializer.Deserialize<List<FilterItem>>(request.Filters)
    : null;

var sorts = request.Sort is not null
    ? JsonSerializer.Deserialize<List<SortItem>>(request.Sort)
    : null;

var results = await _dbContext.Subjects
    .ApplyFilters(filters)
    .ApplySorts(sorts)
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToListAsync();
```

---

### Option 2: Sieve

[Sieve](https://github.com/Bitwarden/Sieve) is a popular .NET filtering/sorting library but uses its own format (`Property==Value,Property2>Value2`), so you would need to transform the DiceUI filter objects on the frontend before sending — not ideal.

---

### Option 3: OData

`Microsoft.AspNetCore.OData` is the most "standard" approach but requires restructuring the API around OData conventions. Heavy for what you need.

---

## Recommendation

Go with **Dynamic LINQ** (`System.Linq.Dynamic.Core`). It is the most natural fit because you keep the DiceUI filter format as-is on the frontend and just deserialize + map on the backend. The column `id` values in the filter objects map directly to your EF Core entity property names (as long as you keep them consistent — e.g. `"code"` → `Subject.Code`).

## Security: Column Whitelist

Sanitize the `id` field against a whitelist of known column names before passing it to Dynamic LINQ to prevent injection:

```csharp
private static readonly HashSet<string> AllowedColumns = ["code", "title", "description", "units"];

// In ApplyFilters, before the switch:
if (!AllowedColumns.Contains(filter.Id)) continue;
```

## DiceUI Filter Operators Reference

| Operator            | Description                        |  Applicable Data Type |
|---------------------|------------------------------------|-----------------------|
| `iLike`             | Case-insensitive contains          | string                |
| `notILike`          | Case-insensitive does not contain  | string                |
| `eq`                | Equals                             | string and numeric    |
| `ne`                | Not equals                         | string and numeric    |
| `lt`                | Less than                          | numeric and datetime  |
| `lte`               | Less than or equal to              | numeric and datetime  |
| `gt`                | Greater than                       | numeric and datetime  |
| `gte`               | Greater than or equal to           | numeric and datetime  |
| `inArray`           | Value is in array                  | string and numeric    |
| `notInArray`        | Value is not in array              | string and numeric    |
| `isEmpty`           | Is null or empty                   | string                |
| `isNotEmpty`        | Is not null or empty               | string                |
| `isBetween`         | Is between two values              | numeric and datetime  |
| `isRelativeToToday` | Relative date (e.g. "last 7 days") | datetime              |

## Frontend: Sending Filters to the Backend

Update `getAllSubjectsPaginatedOptions` in `subject-collection.ts` to accept and forward filters/sort:

```ts
export const getAllSubjectsPaginatedOptions = (
  page: number,
  pageSize: number,
  filters: ExtendedColumnFilter<Subject>[],
  sort: ExtendedColumnSort<Subject>[],
) =>
  createQueryOptions({
    path: "/api/subjects/{page}/{pageSize}",
    pathParams: { page: page.toString(), pageSize: pageSize.toString() },
    params: {
      filters: filters.length ? JSON.stringify(filters) : undefined,
      sort: sort.length ? JSON.stringify(sort) : undefined,
    },
    options: {
      queryKey: [...queryKeys.paginated(page, pageSize), filters, sort],
      staleTime: 1000 * 60 * 2,
      // ... select
    },
  });
```

---

## Architecture Review: Applicability to This Backend (DDD + Clean Architecture)

### Backend Stack

- **Ardalis.Specification** for querying (not raw `IQueryable`)
- **EF Core** via `EfRepository<T>` (`RepositoryBase<T>` from `Ardalis.Specification.EntityFrameworkCore`)
- **Mediator** pattern (queries/handlers in `Enrollify.Application`)
- **FastEndpoints** for the API layer (`Enrollify.WebAPI`)
- **Vogen** value objects (`SubjectCode`, `SubjectId`, etc.)

---

### Why the Dynamic LINQ Approach Needs Adaptation

#### 1. Ardalis.Specification — not raw `IQueryable`

The `ApplyFilters()`/`ApplySorts()` extension methods from Option 1 target raw `IQueryable<T>`. In this architecture, queries are built inside `Specification<T>` (e.g. `ListSubjectsPaginatedSpec`). You **cannot** chain those extension methods directly onto a spec or repository call — they must be adapted to run inside the spec constructor.

#### 2. Vogen value objects break Dynamic LINQ

`Subject.Code` is `SubjectCode` (a Vogen VO), **not** a plain `string`. A Dynamic LINQ expression like:

```csharp
query.Where("Code.ToLower().Contains(@0)", value)
```

will **fail at runtime** because Dynamic LINQ does not understand `SubjectCode`. The existing `SearchSubjectsPaginatedSpec` already handles this with an explicit cast:

```csharp
((string)s.Code).Contains(term)
```

The same pattern must be used for filter expressions on `Code`.

---

### Recommended Approach: Typed Filter Mapping Inside the Specification

Since the filterable columns are finite and known (`code`, `title`, `description`, `units`), use **typed LINQ expressions inside the Specification** instead of Dynamic LINQ strings. This is safer, fully EF Core–translatable, and consistent with the existing spec pattern.

**FilterItem / SortItem records** (place in `Enrollify.Application/Filtering/`):

```csharp
public record FilterItem
{
    public required string Id { get; init; }
    public required string Value { get; init; }
    public required string Variant { get; init; }
    public required string Operator { get; init; }
    public required string FilterId { get; init; }
}

public record SortItem
{
    public required string Id { get; init; }
    public required bool Desc { get; init; }
}
```

**Updated `ListSubjectsPaginatedSpec`:**

```csharp
public class ListSubjectsPaginatedSpec : Specification<Subject>
{
    private static readonly HashSet<string> AllowedColumns = ["code", "title", "description", "units"];

    public ListSubjectsPaginatedSpec(int pageNumber, int pageSize, IEnumerable<FilterItem>? filters, IEnumerable<SortItem>? sorts)
    {
        Query.AsNoTracking().Include(s => s.PreferRoomType);

        foreach (var filter in filters ?? [])
        {
            if (!AllowedColumns.Contains(filter.Id)) continue;

            switch (filter.Id)
            {
                case "code":
                    Query.Where(filter.Operator switch
                    {
                        "iLike"      => s => ((string)s.Code).ToLower().Contains(filter.Value.ToLower()),
                        "notILike"   => s => !((string)s.Code).ToLower().Contains(filter.Value.ToLower()),
                        "eq"         => s => ((string)s.Code) == filter.Value,
                        "ne"         => s => ((string)s.Code) != filter.Value,
                        "isEmpty"    => s => ((string)s.Code) == string.Empty,
                        "isNotEmpty" => s => ((string)s.Code) != string.Empty,
                        _            => s => true
                    });
                    break;
                case "title":
                    Query.Where(filter.Operator switch
                    {
                        "iLike"      => s => s.Title.ToLower().Contains(filter.Value.ToLower()),
                        "notILike"   => s => !s.Title.ToLower().Contains(filter.Value.ToLower()),
                        "eq"         => s => s.Title == filter.Value,
                        "ne"         => s => s.Title != filter.Value,
                        "isEmpty"    => s => string.IsNullOrEmpty(s.Title),
                        "isNotEmpty" => s => !string.IsNullOrEmpty(s.Title),
                        _            => s => true
                    });
                    break;
                case "description":
                    Query.Where(filter.Operator switch
                    {
                        "iLike"      => s => s.Description.ToLower().Contains(filter.Value.ToLower()),
                        "notILike"   => s => !s.Description.ToLower().Contains(filter.Value.ToLower()),
                        "eq"         => s => s.Description == filter.Value,
                        "ne"         => s => s.Description != filter.Value,
                        "isEmpty"    => s => string.IsNullOrEmpty(s.Description),
                        "isNotEmpty" => s => !string.IsNullOrEmpty(s.Description),
                        _            => s => true
                    });
                    break;
                case "units":
                    if (!decimal.TryParse(filter.Value, out var unitsValue)) break;
                    Query.Where(filter.Operator switch
                    {
                        "eq"  => s => s.Units == unitsValue,
                        "ne"  => s => s.Units != unitsValue,
                        "lt"  => s => s.Units < unitsValue,
                        "lte" => s => s.Units <= unitsValue,
                        "gt"  => s => s.Units > unitsValue,
                        "gte" => s => s.Units >= unitsValue,
                        _     => s => true
                    });
                    break;
            }
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

        Query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    private void ApplySort(SortItem sort, bool isFirst)
    {
        switch (sort.Id)
        {
            case "code":
                if (isFirst) { if (sort.Desc) Query.OrderByDescending(s => s.Code); else Query.OrderBy(s => s.Code); }
                else         { if (sort.Desc) Query.ThenByDescending(s => s.Code);  else Query.ThenBy(s => s.Code); }
                break;
            case "title":
                if (isFirst) { if (sort.Desc) Query.OrderByDescending(s => s.Title); else Query.OrderBy(s => s.Title); }
                else         { if (sort.Desc) Query.ThenByDescending(s => s.Title);  else Query.ThenBy(s => s.Title); }
                break;
            case "units":
                if (isFirst) { if (sort.Desc) Query.OrderByDescending(s => s.Units); else Query.OrderBy(s => s.Units); }
                else         { if (sort.Desc) Query.ThenByDescending(s => s.Units);  else Query.ThenBy(s => s.Units); }
                break;
            case "description":
                if (isFirst) { if (sort.Desc) Query.OrderByDescending(s => s.Description); else Query.OrderBy(s => s.Description); }
                else         { if (sort.Desc) Query.ThenByDescending(s => s.Description);  else Query.ThenBy(s => s.Description); }
                break;
        }
    }
}
```

**Updated `ListSubjectsPaginatedQuery`:**

```csharp
public record ListSubjectsPaginatedQuery(
    int page = 1,
    int pageSize = 10,
    IEnumerable<FilterItem>? Filters = null,
    IEnumerable<SortItem>? Sorts = null) : IQuery<Result<PagedResult<SubjectDto>>>;
```

In the handler, pass the spec to `CountAsync` (like `SearchSubjectsPaginatedQuery` does) so total count also respects filters:

```csharp
var spec = new ListSubjectsPaginatedSpec(request.page, request.pageSize, request.Filters, request.Sorts);
var subjects = await _readRepository.ListAsync(spec, cancellationToken);
var totalCount = await _readRepository.CountAsync(spec, cancellationToken); // NOT CountAsync(cancellationToken)
```

**Updated `ListPaginatedSubjectsEndpoint`:**

```csharp
public class ListPaginatedSubjectsRequest
{
    [Microsoft.AspNetCore.Mvc.FromRoute] public required int Page { get; set; }
    [Microsoft.AspNetCore.Mvc.FromRoute] public required int PageSize { get; set; }

    [QueryParam] public string? Filters { get; set; }
    [QueryParam] public string? Sort { get; set; }
}

public class ListPaginatedSubjectsEndpoint(IMediator mediator)
    : Endpoint<ListPaginatedSubjectsRequest, Application.PagedResult<SubjectDto>>
{
    public override async Task HandleAsync(ListPaginatedSubjectsRequest request, CancellationToken cancellationToken)
    {
        var filters = request.Filters is not null
            ? JsonSerializer.Deserialize<List<FilterItem>>(request.Filters)
            : null;

        var sorts = request.Sort is not null
            ? JsonSerializer.Deserialize<List<SortItem>>(request.Sort)
            : null;

        var result = await mediator.Send(
            new ListSubjectsPaginatedQuery(request.Page, request.PageSize, filters, sorts),
            cancellationToken);

        await Send.OkAsync(result.Value);
    }
}
```

---

### Summary: What Changes vs. What the Doc Says

| Item | Doc (Option 1) | This Architecture |
|---|---|---|
| Filter application | `IQueryable<T>` extension | Inside `Specification<T>` constructor |
| Dynamic LINQ strings | ✅ Used for all columns | ❌ Breaks on Vogen VOs — use typed expressions |
| `Code` column | `"Code.ToLower().Contains(@0)"` | `((string)s.Code).ToLower().Contains(...)` |
| Column whitelist | Static `HashSet<string>` | Same, inside the spec |
| `CountAsync` | Not mentioned | Must pass the spec (not just cancellation token) to respect filters |
| `FilterItem`/`SortItem` records | Directly reusable | ✅ Same definitions, place in `Enrollify.Application/Filtering/` |
| Frontend changes | Directly reusable | ✅ No changes needed |
