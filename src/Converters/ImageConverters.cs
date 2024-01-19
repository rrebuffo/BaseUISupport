using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BaseUISupport.Converters;

public class StringToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, CultureInfo? culture)
    {
        object? result = null;

        if (value is string uri)
        {
            try
            {
                BitmapImage image = new();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                if (!File.Exists(uri)) return null;
                image.UriSource = new Uri(uri);
                image.EndInit();
                result = image;
            }
            catch (Exception) { }
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class ByteArrayToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, CultureInfo? culture)
    {
        object? result = null;
        if (value is byte[] data && data.Length > 0)
        {
            try
            {
                BitmapImage image = new();
                using (MemoryStream memStream = new(data))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = memStream;
                    image.EndInit();
                    image.Freeze();
                }
                result = image;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Data);
            }
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
