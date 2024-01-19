﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class ListBoxSelectedConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((int)value >= 0) return true;
        else return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;
    }
}