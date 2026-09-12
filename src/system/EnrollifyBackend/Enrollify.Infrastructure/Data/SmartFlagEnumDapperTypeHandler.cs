using Ardalis.SmartEnum;
using Dapper;
using System.Data;

namespace Enrollify.Infrastructure.Data;

public class SmartFlagEnumDapperTypeHandler<T> : SqlMapper.TypeHandler<T>
    where T : SmartFlagEnum<T>
{
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        if (value is null)
        {
            throw new DataException($"{typeof(T).Name} cannot be written as a null SmartFlagEnum value.");
        }

        parameter.Value = value.Value;
    }

    public override T Parse(object value)
    {
        if (value is null || value is DBNull)
        {
            throw new DataException($"{typeof(T).Name} cannot be read from a null database value.");
        }

        int intValue;
        try
        {
            intValue = Convert.ToInt32(value);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            throw new DataException(
                $"Unable to convert database value '{value}' ({value.GetType().FullName}) to {typeof(T).Name}. Expected a valid Int32 flag value.",
                ex);
        }

        try
        {
            return SmartFlagEnum<T>.DeserializeValue(intValue);
        }
        catch (Exception ex)
        {
            throw new DataException(
                $"Database value '{intValue}' is not a valid {typeof(T).Name} SmartFlagEnum value.",
                ex);
        }
    }
}