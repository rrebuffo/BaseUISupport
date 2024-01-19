using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BaseUISupport.Helpers;

public class CalendarHelper
{
    public static readonly DependencyProperty ReleaseMouseProperty = DependencyProperty.RegisterAttached("ReleaseMouse", typeof(bool), typeof(CalendarHelper), new PropertyMetadata(false, ReleaseMouseChangedCallback));

    private static void ReleaseMouseChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) throw new Exception("Attached property must be used with UIElement.");

        if ((bool)e.NewValue) element.PreviewMouseUp += Element_PreviewMouseUp;
        else element.PreviewMouseUp -= Element_PreviewMouseUp;
    }

    private static void Element_PreviewMouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (Mouse.Captured is CalendarItem item) item.ReleaseMouseCapture();
    }

    public static void SetReleaseMouse(UIElement element, bool value) => element.SetValue(ReleaseMouseProperty, value);
    public static bool GetReleaseMouse(UIElement element) => (bool)element.GetValue(ReleaseMouseProperty);
}
