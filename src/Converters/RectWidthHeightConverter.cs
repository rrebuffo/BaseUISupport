using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class RectWidthHeightConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        object value1 = value[0];
        object value2 = value[1];
        if (!(value1 is double && value2 is double)) return new Rect(0, 0, 0, 0);

        var width = (double)value[0];
        var height = (double)value[1];
        return new Rect(0, 0, width, height);

    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
