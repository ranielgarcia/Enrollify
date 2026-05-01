using System.Linq.Expressions;
using Ardalis.Specification;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.Teachers.Specifications;

public class FilterTeachersPaginatedSpec : Specification<Teacher>
{
    private static readonly HashSet<string> AllowedFilterColumns =
        [
            "fullname",
            nameof(Teacher.TeacherIdentifier).ToLower(),
            nameof(Teacher.Email).ToLower(),
            nameof(Teacher.PhoneNumber).ToLower(),
            nameof(Teacher.Department).ToLower(),
            nameof(Teacher.AcademicTitle).ToLower(),
            nameof(Teacher.Qualification).ToLower(),
            nameof(Teacher.Specialization).ToLower(),
            nameof(Teacher.OfficeLocation).ToLower(),
            nameof(Teacher.OfficeHours).ToLower(),
            nameof(Teacher.Biography).ToLower()
        ];

    // The explicit casts `(object?)t.TeacherIdentifier` etc. box Vogen value objects; EF Core still generates correct SQL.
    private static readonly IReadOnlyDictionary<string, Expression<Func<Teacher, object?>>> SortMap =
        new Dictionary<string, Expression<Func<Teacher, object?>>>
        {
            ["fullname"]         = t => new { t.FirstName, t.MiddleName, t.LastName},
            ["teacheridentifier"] = t => (object?)t.TeacherIdentifier,
            ["email"]             = t => (object?)t.Email,
            ["academictitle"]     = t => t.AcademicTitle,
            ["qualification"]     = t => t.Qualification,
            ["specialization"]    = t => t.Specialization,
            ["officelocation"]    = t => t.OfficeLocation
        };

    private readonly SpecSortBuilder<Teacher> _sortBuilder = new();

    public FilterTeachersPaginatedSpec(int pageNumber, int pageSize, IEnumerable<FilterItem>? filters, IEnumerable<SortItem>? sorts, JoinOperator joinOperator = JoinOperator.And)
    {
        // Explicit casts like `(string)t.TeacherIdentifier` leverage Vogen's generated explicit operator
        // to let EF Core resolve them to the underlying string column.
        // No EF Core dependency needed — only Ardalis.Specification.
        Query
            .AsNoTracking()
            .Include(t => t.Subjects.Where(s => s.IsActive))
            .Include(r => r.CreatedByUser)
            .Include(r => r.UpdatedByUser)
            .Include(t => t.Department);

        var filterExpressions = new List<Expression<Func<Teacher, bool>>>();

        foreach (var filter in filters ?? [])
        {
            var filterId = filter.Id.ToLower();
            if (!AllowedFilterColumns.Contains(filterId, StringComparer.InvariantCultureIgnoreCase)) continue;

            var expr = filterId switch
            {
                "fullname" => FilterExpressionBuilder.ForString<Teacher>(t => t.FirstName + " " + t.MiddleName + " " + t.LastName, filter),

                "department" => FilterExpressionBuilder.ForNullableString<Teacher>(t => t.Department != null ? t.Department.Name : null, filter),
                
                // Vogen value objects (explicit cast to string)
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
            Query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);
        }

        Query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
