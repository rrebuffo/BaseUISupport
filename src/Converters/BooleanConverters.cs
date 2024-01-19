using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class BooleanConverter<T> : IValueConverter
{

    public BooleanConverter(T trueValue, T falseValue)
    {
        True = trueValue;
        False = falseValue;
    }

    public T True { get; set; }
    public T False { get; set; }

    public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolean && boolean ? True : False;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is T t && EqualityComparer<T>.Default.Equals(t, True);
    }
}

public class InvertBooleanConverter : IValueConverter
{

    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }
}

public class BooleanOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not bool value1 || values[1] is not bool value2) return false;
        return (value1 || value2);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanAndConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not bool value1 || values[1] is not bool value2) return false;
        return (value1 && value2);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
{
    public BooleanToVisibilityConverter() :
        base(Visibility.Visible, Visibility.Collapsed)
    { }
}

public class BoolToValueConverter<T> : IValueConverter
{
    public T? FalseValue { get; set; }
    public T? TrueValue { get; set; }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (TrueValue is null || FalseValue is null) return null;
        if (value is null) return FalseValue;
        else return (bool)value ? TrueValue : FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && value.Equals(TrueValue);
    }
}

public class StringToBoolConverter : IValueConverter
{
    public string? FalseValue { get; set; }
    public string? TrueValue { get; set; }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (TrueValue is null || FalseValue is null) return null;
        if (value is null) return FalseValue;
        else return (bool)value ? TrueValue : FalseValue;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && value.Equals(TrueValue);
    }
}

public class BoolToStringConverter : BoolToValueConverter<string> { }
