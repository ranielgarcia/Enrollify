using Ardalis.SmartEnum;
using Dapper;
using Enrollify.Core.Constants;
using System.Data;

namespace Enrollify.Infrastructure.Data;

/// <summary>
/// Custom Dapper type handler for DayOfWeekEnum.
/// This handler maps between the database string value ("MON", "TUE", etc.)
/// and the DayOfWeekEnum SmartEnum instance.
/// </summary>
public class DayOfWeekEnumDapperTypeHandler : SqlMapper.TypeHandler<DayOfWeekEnum>
{
  public override void SetValue(IDbDataParameter parameter, DayOfWeekEnum? value)
  {
    parameter.Value = value?.Value ?? (object)DBNull.Value;
  }

  public override DayOfWeekEnum Parse(object value)
  {
    if (value == null || value is DBNull)
    {
      throw new ArgumentNullException(nameof(value), "DayOfWeek cannot be null");
    }

    var stringValue = value.ToString();
    if (string.IsNullOrWhiteSpace(stringValue))
    {
      throw new ArgumentException("DayOfWeek value cannot be empty", nameof(value));
    }

    // Use FromValue to match the database value ("MON", "TUE", etc.)
    return SmartEnum<DayOfWeekEnum, string>.FromValue(stringValue);
  }
}
