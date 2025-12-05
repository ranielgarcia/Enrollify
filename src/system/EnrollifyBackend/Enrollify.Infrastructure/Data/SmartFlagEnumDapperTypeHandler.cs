using Ardalis.SmartEnum;
using Dapper;
using System.Data;

namespace Enrollify.Infrastructure.Data;

public class SmartFlagEnumDapperTypeHandler<T> : SqlMapper.TypeHandler<IEnumerable<T>>
    where T : SmartFlagEnum<T, int>
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value.Value; // store the integer flags
    }

    public override IEnumerable<T> Parse(object value)
    {
        // value comes as int, long, or other numeric types
        var intValue = Convert.ToInt32(value);
        return SmartFlagEnum<T, int>.FromValue(intValue);
    }
}