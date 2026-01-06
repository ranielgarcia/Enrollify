using System.ComponentModel;
using System.Globalization;
using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

[TypeConverter(typeof(AcademicSystemEnumTypeConverter))]
public sealed class AcademicSystemEnum : SmartEnum<AcademicSystemEnum>
{
    public static readonly AcademicSystemEnum Semester = new("Semester", 2);
    public static readonly AcademicSystemEnum Trimester = new("Trimester", 3);

    private AcademicSystemEnum(string name, int value) : base(name, value) { }
}

public class AcademicSystemEnumTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue && AcademicSystemEnum.TryFromName(stringValue, ignoreCase: true, out var result))
        {
            return result;
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is AcademicSystemEnum enumValue)
        {
            return enumValue.Name;
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
