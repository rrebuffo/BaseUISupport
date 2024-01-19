using System;
using System.Globalization;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class SelectedItemsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int items = (int)value;

        if (items == 0) return "No selected elements.";
        if (items == 1) return "One element selected.";
        return items + " elements selected.";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
