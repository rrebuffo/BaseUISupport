using System;
using System.Globalization;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class EqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {

        if (values.Length < 2 || values[0] == null || values[1] == null)
            return false;
        return values[0].Equals(values[1]);

    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InequalityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {

        if (values.Length < 2 || values[0] == null || values[1] == null)
            return false;

        return !values[0].Equals(values[1]);

    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
