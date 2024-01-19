using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BaseUISupport.Converters;

public class IcoToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        Icon? icon;
        if (parameter is int size) {
            int iconsize = 16;
            if (size >= 20) iconsize = 20;
            if (size >= 24) iconsize = 24;
            if (size >= 32) iconsize = 32;
            if (size >= 48) iconsize = 256;
            icon = new((Icon)value, new(iconsize, iconsize));
        } else
        {
            icon = (Icon)value;
        }
        return IconConverter.ToImageSource(icon);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public static class IconConverter
{
    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    public static ImageSource? ToImageSource(Icon icon)
    {
        if (icon is null) return null;
        Bitmap? bitmap = icon?.ToBitmap();
        if (bitmap is null) return null;
        IntPtr hBitmap = bitmap.GetHbitmap();

        ImageSource? wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            hBitmap,
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        if (!DeleteObject(hBitmap))
        {
            throw new Win32Exception();
        }

        return wpfBitmap;
    }

    public static Bitmap? GetJumboFromResource(string resourceUri)
    {
        Bitmap? sized_icon = null;
        try
        {
            using MemoryStream? source_stream = new();
            Application.GetResourceStream(new(resourceUri)).Stream.CopyTo(source_stream);
            byte[] buffer = source_stream.ToArray();

            const int ICONDIR = 6;
            const int ICONDIRENTRY = 16;

            for (int i = 0; i < BitConverter.ToInt16(buffer, 4); i++)
            {
                int width = buffer[ICONDIR + ICONDIRENTRY * i];
                int height = buffer[ICONDIR + ICONDIRENTRY * i + 1];
                int iBitCount = BitConverter.ToInt16(buffer, ICONDIR + ICONDIRENTRY * i + 6);
                if (width == 0 && height == 0 && iBitCount == 32)
                {
                    int size = BitConverter.ToInt32(buffer, ICONDIR + ICONDIRENTRY * i + 8);
                    int offset = BitConverter.ToInt32(buffer, ICONDIR + ICONDIRENTRY * i + 12);
                    MemoryStream? target_stream = new();
                    BinaryWriter? writer = new(target_stream);
                    writer.Write(buffer, offset, size);
                    target_stream.Seek(0, SeekOrigin.Begin);
                    sized_icon = new(target_stream);
                    break;
                }
            }
        }
        catch { }
        return sized_icon;
    }

    public static BitmapImage? GetJumboImageFromResource(string resourceUri)
    {
        Bitmap? source = GetJumboFromResource(resourceUri);
        if (source is null) return null;
        MemoryStream? source_bmp = new();
        source.Save(source_bmp, ImageFormat.Png);
        BitmapImage? target = new();
        target.BeginInit();
        source_bmp.Seek(0, SeekOrigin.Begin);
        target.StreamSource = source_bmp;
        target.EndInit();
        return target;
    }
}
