using System;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class TimeSpanToMillisecondsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return ((TimeSpan)value).TotalMilliseconds;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return TimeSpan.FromMilliseconds(double.Parse((string)value));
    }
}

public class TimeSpanToSecondsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return ((TimeSpan)value).TotalSeconds;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return TimeSpan.FromSeconds(double.Parse((string)value));
    }
}
