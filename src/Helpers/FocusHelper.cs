using System;
using System.Windows;
using System.Windows.Controls;

namespace BaseUISupport.Helpers;

public class FocusHelper
{
    public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(FocusHelper), new PropertyMetadata(false, SelectAllOnFocusChangedCallback));

    private static void SelectAllOnFocusChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) throw new Exception("Attached property must be used with UIElement.");

        if ((bool)e.NewValue)
        {
            element.GotKeyboardFocus += OnGotKeyboardFocus;
            element.GotFocus += OnGotFocus;
        }
        else
        {
            element.GotFocus -= OnGotFocus;
        }
    }

    private static void OnGotKeyboardFocus(object? sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox target)
        {
            target.SelectAll();
        }
    }

    private static void OnGotFocus(object? sender, RoutedEventArgs e)
    {
        if(sender is TextBox target)
        {
            target.SelectAll();
            target.LostMouseCapture += OnLostMouseCapture;
        }
    }

    private static void OnLostMouseCapture(object? sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not TextBox target) return;
        target.LostMouseCapture -= OnLostMouseCapture;
        target.SelectAll();
    }

    public static void SetSelectAllOnFocus(UIElement element, bool value) => element.SetValue(SelectAllOnFocusProperty, value);
    public static bool GetSelectAllOnFocus(UIElement element) => (bool)element.GetValue(SelectAllOnFocusProperty);
}
