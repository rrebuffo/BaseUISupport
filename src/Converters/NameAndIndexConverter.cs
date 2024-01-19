﻿using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class NameAndIndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {

        if (values[0] is string && values[1] is ListBox && values[2] is object)
        {
            int index = ((ListBox)values[1]).Items.IndexOf(((ListBoxItem)values[2]).DataContext);
            return string.Format((string)values[0], index+1);
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
