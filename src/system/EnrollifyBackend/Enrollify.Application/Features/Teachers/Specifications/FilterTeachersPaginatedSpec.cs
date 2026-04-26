using System.Linq.Expressions;
using Ardalis.Specification;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.Teachers.Specifications;

public class FilterTeachersPaginatedSpec : Specification<Teacher>
{
    private static readonly HashSet<string> AllowedFilterColumns =
        [
            nameof(Teacher.FirstName).ToLower(),
            nameof(Teacher.MiddleName).ToLower(),
            nameof(Teacher.LastName).ToLower(),
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

    public FilterTeachersPaginatedSpec(int pageNumber, int pageSize, IEnumerable<FilterItem>? filters, IEnumerable<SortItem>? sorts, JoinOperator joinOperator = JoinOperator.and)
    {
        // The explicit cast `((string)t.TeacherIdentifier).Contains(searchTerm)` leverages Vogen's generated explicit operator
        // to let EF Core resolve it to the underlying string column.
        // No EF Core dependency needed — only Ardalis.Specification.
        Query
            .AsNoTracking()
            .Include(t => t.Department);

        var filterExpressions = new List<Expression<Func<Teacher, bool>>>();

        foreach (var filter in filters ?? [])
        {
            if (!AllowedFilterColumns.Contains(filter.Id)) continue;

            switch (filter.Id)
            {
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
                case "middlename":
                    Expression<Func<Teacher, bool>>? middleNameExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.MiddleName.Contains(filter.Value),
                        FilterOperator.NotILike => t => !t.MiddleName.Contains(filter.Value),
                        FilterOperator.Eq => t => t.MiddleName == filter.Value,
                        FilterOperator.Ne => t => t.MiddleName != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.MiddleName),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.MiddleName),
                        _ => null
                    };
                    if (middleNameExpr is not null) filterExpressions.Add(middleNameExpr);
                    break;
                case "lastname":
                    Expression<Func<Teacher, bool>>? lastNameExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.LastName.Contains(filter.Value),
                        FilterOperator.NotILike => t => !t.LastName.Contains(filter.Value),
                        FilterOperator.Eq => t => t.LastName == filter.Value,
                        FilterOperator.Ne => t => t.LastName != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.LastName),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.LastName),
                        _ => null
                    };
                    if (lastNameExpr is not null) filterExpressions.Add(lastNameExpr);
                    break;
                case "teacheridentifier":
                    Expression<Func<Teacher, bool>>? identifierExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => ((string)t.TeacherIdentifier).Contains(filter.Value),
                        FilterOperator.NotILike => t => !((string)t.TeacherIdentifier).Contains(filter.Value),
                        FilterOperator.Eq => t => ((string)t.TeacherIdentifier) == filter.Value,
                        FilterOperator.Ne => t => ((string)t.TeacherIdentifier) != filter.Value,
                        FilterOperator.IsEmpty => t => ((string)t.TeacherIdentifier) == string.Empty,
                        FilterOperator.IsNotEmpty => t => ((string)t.TeacherIdentifier) != string.Empty,
                        _ => null
                    };
                    if (identifierExpr is not null) filterExpressions.Add(identifierExpr);
                    break;
                case "email":
                    Expression<Func<Teacher, bool>>? emailExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => ((string)t.Email).Contains(filter.Value),
                        FilterOperator.NotILike => t => !((string)t.Email).Contains(filter.Value),
                        FilterOperator.Eq => t => ((string)t.Email) == filter.Value,
                        FilterOperator.Ne => t => ((string)t.Email) != filter.Value,
                        FilterOperator.IsEmpty => t => ((string)t.Email) == string.Empty,
                        FilterOperator.IsNotEmpty => t => ((string)t.Email) != string.Empty,
                        _ => null
                    };
                    if (emailExpr is not null) filterExpressions.Add(emailExpr);
                    break;
                case "phonenumber":
                    Expression<Func<Teacher, bool>>? phoneExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => ((string)t.PhoneNumber).Contains(filter.Value),
                        FilterOperator.NotILike => t => !((string)t.PhoneNumber).Contains(filter.Value),
                        FilterOperator.Eq => t => ((string)t.PhoneNumber) == filter.Value,
                        FilterOperator.Ne => t => ((string)t.PhoneNumber) != filter.Value,
                        FilterOperator.IsEmpty => t => ((string)t.PhoneNumber) == string.Empty,
                        FilterOperator.IsNotEmpty => t => ((string)t.PhoneNumber) != string.Empty,
                        _ => null
                    };
                    if (phoneExpr is not null) filterExpressions.Add(phoneExpr);
                    break;
                case "academictitle":
                    Expression<Func<Teacher, bool>>? academicTitleExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.AcademicTitle != null && t.AcademicTitle.Contains(filter.Value),
                        FilterOperator.NotILike => t => t.AcademicTitle == null || !t.AcademicTitle.Contains(filter.Value),
                        FilterOperator.Eq => t => t.AcademicTitle == filter.Value,
                        FilterOperator.Ne => t => t.AcademicTitle != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.AcademicTitle),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.AcademicTitle),
                        _ => null
                    };
                    if (academicTitleExpr is not null) filterExpressions.Add(academicTitleExpr);
                    break;
                case "qualification":
                    Expression<Func<Teacher, bool>>? qualificationExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.Qualification != null && t.Qualification.Contains(filter.Value),
                        FilterOperator.NotILike => t => t.Qualification == null || !t.Qualification.Contains(filter.Value),
                        FilterOperator.Eq => t => t.Qualification == filter.Value,
                        FilterOperator.Ne => t => t.Qualification != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.Qualification),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.Qualification),
                        _ => null
                    };
                    if (qualificationExpr is not null) filterExpressions.Add(qualificationExpr);
                    break;
                case "specialization":
                    Expression<Func<Teacher, bool>>? specializationExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.Specialization != null && t.Specialization.Contains(filter.Value),
                        FilterOperator.NotILike => t => t.Specialization == null || !t.Specialization.Contains(filter.Value),
                        FilterOperator.Eq => t => t.Specialization == filter.Value,
                        FilterOperator.Ne => t => t.Specialization != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.Specialization),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.Specialization),
                        _ => null
                    };
                    if (specializationExpr is not null) filterExpressions.Add(specializationExpr);
                    break;
                case "officelocation":
                    Expression<Func<Teacher, bool>>? officeLocationExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.OfficeLocation != null && t.OfficeLocation.Contains(filter.Value),
                        FilterOperator.NotILike => t => t.OfficeLocation == null || !t.OfficeLocation.Contains(filter.Value),
                        FilterOperator.Eq => t => t.OfficeLocation == filter.Value,
                        FilterOperator.Ne => t => t.OfficeLocation != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.OfficeLocation),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.OfficeLocation),
                        _ => null
                    };
                    if (officeLocationExpr is not null) filterExpressions.Add(officeLocationExpr);
                    break;
                case "officehours":
                    Expression<Func<Teacher, bool>>? officeHoursExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.OfficeHours != null && t.OfficeHours.Contains(filter.Value),
                        FilterOperator.NotILike => t => t.OfficeHours == null || !t.OfficeHours.Contains(filter.Value),
                        FilterOperator.Eq => t => t.OfficeHours == filter.Value,
                        FilterOperator.Ne => t => t.OfficeHours != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.OfficeHours),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.OfficeHours),
                        _ => null
                    };
                    if (officeHoursExpr is not null) filterExpressions.Add(officeHoursExpr);
                    break;
                case "biography":
                    Expression<Func<Teacher, bool>>? biographyExpr = filter.Operator switch
                    {
                        FilterOperator.ILike => t => t.Biography != null && t.Biography.Contains(filter.Value),
                        FilterOperator.NotILike => t => t.Biography == null || !t.Biography.Contains(filter.Value),
                        FilterOperator.Eq => t => t.Biography == filter.Value,
                        FilterOperator.Ne => t => t.Biography != filter.Value,
                        FilterOperator.IsEmpty => t => string.IsNullOrEmpty(t.Biography),
                        FilterOperator.IsNotEmpty => t => !string.IsNullOrEmpty(t.Biography),
                        _ => null
                    };
                    if (biographyExpr is not null) filterExpressions.Add(biographyExpr);
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
                return Expression.Lambda<Func<Teacher, bool>>(body, param);
            });
            Query.Where(combined);
        }

        var sortList = sorts?.ToList() ?? [];
        if (sortList.Count > 0)
        {
            ApplySort(sortList[0], isFirst: true);
            for (int i = 1; i < sortList.Count; i++)
                ApplySort(sortList[i], isFirst: false);
        }
        else
        {
            Query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);
        }

        Query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    private IOrderedSpecificationBuilder<Teacher>? _orderedQuery;

    private void ApplySort(SortItem sort, bool isFirst)
    {
        if (isFirst)
        {
            _orderedQuery = sort.Id switch
            {
                "firstname"        => sort.Desc ? Query.OrderByDescending(t => t.FirstName)        : Query.OrderBy(t => t.FirstName),
                "middlename"       => sort.Desc ? Query.OrderByDescending(t => t.MiddleName)       : Query.OrderBy(t => t.MiddleName),
                "lastname"         => sort.Desc ? Query.OrderByDescending(t => t.LastName)         : Query.OrderBy(t => t.LastName),
                "teacheridentifier"=> sort.Desc ? Query.OrderByDescending(t => t.TeacherIdentifier): Query.OrderBy(t => t.TeacherIdentifier),
                "email"            => sort.Desc ? Query.OrderByDescending(t => t.Email)            : Query.OrderBy(t => t.Email),
                "academictitle"    => sort.Desc ? Query.OrderByDescending(t => t.AcademicTitle)    : Query.OrderBy(t => t.AcademicTitle),
                "qualification"    => sort.Desc ? Query.OrderByDescending(t => t.Qualification)    : Query.OrderBy(t => t.Qualification),
                "specialization"   => sort.Desc ? Query.OrderByDescending(t => t.Specialization)   : Query.OrderBy(t => t.Specialization),
                "officelocation"   => sort.Desc ? Query.OrderByDescending(t => t.OfficeLocation)   : Query.OrderBy(t => t.OfficeLocation),
                _                  => null
            };
        }
        else if (_orderedQuery is not null)
        {
            _orderedQuery = sort.Id switch
            {
                "firstname"        => sort.Desc ? _orderedQuery.ThenByDescending(t => t.FirstName)        : _orderedQuery.ThenBy(t => t.FirstName),
                "middlename"       => sort.Desc ? _orderedQuery.ThenByDescending(t => t.MiddleName)       : _orderedQuery.ThenBy(t => t.MiddleName),
                "lastname"         => sort.Desc ? _orderedQuery.ThenByDescending(t => t.LastName)         : _orderedQuery.ThenBy(t => t.LastName),
                "teacheridentifier"=> sort.Desc ? _orderedQuery.ThenByDescending(t => t.TeacherIdentifier): _orderedQuery.ThenBy(t => t.TeacherIdentifier),
                "email"            => sort.Desc ? _orderedQuery.ThenByDescending(t => t.Email)            : _orderedQuery.ThenBy(t => t.Email),
                "academictitle"    => sort.Desc ? _orderedQuery.ThenByDescending(t => t.AcademicTitle)    : _orderedQuery.ThenBy(t => t.AcademicTitle),
                "qualification"    => sort.Desc ? _orderedQuery.ThenByDescending(t => t.Qualification)    : _orderedQuery.ThenBy(t => t.Qualification),
                "specialization"   => sort.Desc ? _orderedQuery.ThenByDescending(t => t.Specialization)   : _orderedQuery.ThenBy(t => t.Specialization),
                "officelocation"   => sort.Desc ? _orderedQuery.ThenByDescending(t => t.OfficeLocation)   : _orderedQuery.ThenBy(t => t.OfficeLocation),
                _                  => _orderedQuery
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
