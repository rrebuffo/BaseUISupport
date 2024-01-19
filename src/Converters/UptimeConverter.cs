using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace BaseUISupport.Converters;

public class UptimeConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        StringBuilder sb = new();
        if(value[0] is TimeSpan uptime && value[1] is string days_format && value[2] is string time_format)
        {
            if(uptime.TotalDays>=1)
            {
                sb.AppendFormat(days_format,uptime);
            }
            sb.AppendFormat(time_format, uptime);
        }
        return sb.ToString();
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
