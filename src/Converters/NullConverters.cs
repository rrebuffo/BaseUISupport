using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BaseUISupport.Converters
{
    public class NullConverter<T> : IValueConverter
    {

        public NullConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class NullToVisibilityConverter : NullConverter<Visibility>
    {
        public NullToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed) { }
    }

    public sealed class InvertedNullToVisibilityConverter : NullConverter<Visibility>
    {
        public InvertedNullToVisibilityConverter() : base(Visibility.Collapsed, Visibility.Visible) { }
    }

    public sealed class NullToBooleanConverter : NullConverter<bool>
    {
        public NullToBooleanConverter() : base(true, false) { }
    }

    public sealed class InvertedNullToBooleanConverter : NullConverter<bool>
    {
        public InvertedNullToBooleanConverter() : base(false, true) { }
    }
}
