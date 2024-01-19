using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Resources;


namespace BaseUISupport.Converters;

public class LocalizedEnumConverter : IValueConverter
{
    private static string? GetEnumDescription(Enum enumObj)
    {
        if (enumObj.GetType().GetField(enumObj.ToString()) is not FieldInfo fieldInfo) return null;

        object[] attribArray = fieldInfo.GetCustomAttributes(false);

        if (attribArray.Length == 0)
        {
            return fieldInfo.Name;
        }
        else
        {
            try
            {

                if (attribArray[0] is DisplayAttribute attrib && attrib.Description is not null && attrib.ResourceType is not null)
                {
                    ResourceManager rm = new(attrib.ResourceType.ToString(), attrib.ResourceType.Assembly);
                    var rs = rm.GetResourceSet(CultureInfo.CurrentCulture, false, true);
                    return rm.GetString(attrib.Description) ?? fieldInfo.Name;
                }
                else return fieldInfo.Name;
            }
            catch
            {
                return fieldInfo.Name;
            }
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
