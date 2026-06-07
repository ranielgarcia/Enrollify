using Dapper;
using System.Data;

namespace Enrollify.Infrastructure.Data;

/// <summary>
/// Custom Dapper type handler that converts SQL Server TIME columns (stored as TimeSpan)
/// to .NET TimeOnly type. This is necessary because Dapper doesn't have built-in support
/// for TimeSpan → TimeOnly conversion, which causes deserialization errors when querying
/// time values directly via raw SQL (Dapper).
/// 
/// EF Core handles this automatically through its type conversion system, but Dapper
/// (used in repositories for performance-critical raw SQL queries) requires explicit handling.
/// </summary>
public class TimeSpanToTimeOnlyDapperTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        // When sending TimeOnly to SQL Server, convert it to TimeSpan
        parameter.Value = value.ToTimeSpan();
    }

    public override TimeOnly Parse(object value)
    {
        // When reading from SQL Server (TimeSpan), convert to TimeOnly
        if (value is TimeSpan timeSpan)
        {
            return TimeOnly.FromTimeSpan(timeSpan);
        }

        // Fallback: if somehow we get a TimeOnly directly
        if (value is TimeOnly timeOnly)
        {
            return timeOnly;
        }

        // If we get null or another unexpected type, throw
        throw new DataException(
            $"Unable to convert type {value?.GetType().Name ?? "null"} to TimeOnly. " +
            "Expected TimeSpan from SQL Server TIME column or TimeOnly value.");
    }
}
