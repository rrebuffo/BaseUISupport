using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class EnumConverter : IValueConverter
{
    private static string? GetEnumDescription(Enum enumObj)
    {
        Debug.WriteLine(CultureInfo.CurrentCulture);
        if (enumObj.GetType().GetField(enumObj.ToString()) is not FieldInfo fieldInfo) return null;

        object[] attribArray = fieldInfo.GetCustomAttributes(false);

        if (attribArray.Length == 0)
        {
            return enumObj.ToString();
        }
        else
        {
            if (attribArray[0] is DescriptionAttribute attrib) return attrib.Description;
            else return null;
        }
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum myEnum)
        {
            return GetEnumDescription(myEnum);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.Empty;
    }
}
