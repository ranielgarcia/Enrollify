using System.Linq.Expressions;

namespace Enrollify.Application.Filtering;

public static class FilterExpressionBuilder
{
    /// <summary>
    /// Builds a filter expression for non-nullable string properties.
    /// Also works for Vogen value objects when the caller provides an explicit cast: t => (string)t.Email
    /// </summary>
    public static Expression<Func<T, bool>>? ForString<T>(
        Expression<Func<T, string>> selector,
        FilterItem filter)
    {
        return filter.Operator switch
        {
            FilterOperator.ILike      => Compose(selector, s => s.Contains(filter.Value)),
            FilterOperator.NotILike   => Compose(selector, s => !s.Contains(filter.Value)),
            FilterOperator.Eq         => Compose(selector, s => s == filter.Value),
            FilterOperator.Ne         => Compose(selector, s => s != filter.Value),
            FilterOperator.IsEmpty    => Compose(selector, s => s == string.Empty),
            FilterOperator.IsNotEmpty => Compose(selector, s => s != string.Empty),
            _ => null
        };
    }

    /// <summary>
    /// Builds a filter expression for nullable string? properties.
    /// Handles null checks for ILike/NotILike operators.
    /// </summary>
    public static Expression<Func<T, bool>>? ForNullableString<T>(
        Expression<Func<T, string?>> selector,
        FilterItem filter)
    {
        return filter.Operator switch
        {
            FilterOperator.ILike      => Compose(selector, s => s != null && s.Contains(filter.Value)),
            FilterOperator.NotILike   => Compose(selector, s => s == null || !s.Contains(filter.Value)),
            FilterOperator.Eq         => Compose(selector, s => s == filter.Value),
            FilterOperator.Ne         => Compose(selector, s => s != filter.Value),
            FilterOperator.IsEmpty    => Compose(selector, s => string.IsNullOrEmpty(s)),
            FilterOperator.IsNotEmpty => Compose(selector, s => !string.IsNullOrEmpty(s)),
            _ => null
        };
    }

    /// <summary>
    /// Builds a filter expression for numeric properties.
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
            FilterOperator.Eq  => Compose(selector, v => v.CompareTo(parsedValue) == 0),
            FilterOperator.Ne  => Compose(selector, v => v.CompareTo(parsedValue) != 0),
            FilterOperator.Lt  => Compose(selector, v => v.CompareTo(parsedValue) < 0),
            FilterOperator.Lte => Compose(selector, v => v.CompareTo(parsedValue) <= 0),
            FilterOperator.Gt  => Compose(selector, v => v.CompareTo(parsedValue) > 0),
            FilterOperator.Gte => Compose(selector, v => v.CompareTo(parsedValue) >= 0),
            _ => null
        };
    }

    /// <summary>
    /// Combines multiple filter expressions using AND Or OR logic.
    /// Returns null if the list is empty.
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
            var body = joinOperator == JoinOperator.Or
                ? Expression.OrElse(left.Body, rightBody)
                : Expression.AndAlso(left.Body, rightBody);
            return Expression.Lambda<Func<T, bool>>(body, param);
        });
    }

    // Composes a property selector with a predicate condition into a single entity predicate.
    private static Expression<Func<T, bool>> Compose<T, TProp>(
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

    private sealed class ExpressionParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);

        public static Expression Replace(Expression body, ParameterExpression from, Expression to)
            => new ExpressionParameterReplacer(from, to).Visit(body);
    }
}
