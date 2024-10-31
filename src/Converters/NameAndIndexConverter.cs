using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class NameAndIndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is string format && values[1] is IList list && values[2] is object target)
        {
            int index = list.IndexOf(target);
            return string.Format(format, index + 1);
        }
        else if (values[0] is string && values[1] is ListBox && values[2] is object)
        {
            int index = ((ListBox)values[1]).Items.IndexOf(((ListBoxItem)values[2]).DataContext);
            return string.Format((string)values[0], index + 1);
        }
        else if (values[0] is string fallback)
        {
            return string.Format(fallback, "X");
        }
        else
        {
            return "";
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {

        if (values[0] is ListBox list && values[1] is object item)
        {
            int index = list.Items.IndexOf(item);
            return (index + 1).ToString();
        }
        else
        {
            return "";
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
