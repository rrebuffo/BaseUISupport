using System;
using System.Globalization;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class NullComboBoxConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && parameter is string param && str == param) return null;
        else return value;
    }
}