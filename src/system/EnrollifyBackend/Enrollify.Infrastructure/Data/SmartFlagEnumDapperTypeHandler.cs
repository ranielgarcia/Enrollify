using Ardalis.SmartEnum;
using Dapper;
using System.Data;

namespace Enrollify.Infrastructure.Data;

public class SmartFlagEnumDapperTypeHandler<T> : SqlMapper.TypeHandler<T>
    where T : SmartFlagEnum<T>
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value.Value;
    }

    public override T Parse(object value)
    {
        var intValue = Convert.ToInt32(value);

        return SmartFlagEnum<T>.DeserializeValue(intValue);
    }
}