using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BaseUISupport.Helpers;

public class TreeViewHelper
{
    private static TreeViewItem? _currentItem = null;
    private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseDirectlyOverItem", typeof(bool), typeof(TreeViewHelper), new(false, new(IsMouseDirectlyOverItemChanged), new(CalculateIsMouseDirectlyOverItem)));

    public static readonly DependencyProperty IsMouseDirectlyOverItemProperty = IsMouseDirectlyOverItemKey.DependencyProperty;
    public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
    }

    private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
    {
        if (item == _currentItem) return true;
        else return false;
    }

    private static void IsMouseDirectlyOverItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent("UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewHelper));

    static TreeViewHelper()
    {
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseEnterEvent, new MouseEventHandler(OnMouseTransition), true);
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseLeaveEvent, new MouseEventHandler(OnMouseTransition), true);
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UpdateOverItemEvent, new RoutedEventHandler(OnUpdateOverItem));
    }

    static void OnUpdateOverItem(object? sender, RoutedEventArgs args)
    {
        if(sender is not TreeViewItem _currentItem) return;

        _currentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
        args.Handled = true;
    }

    static void OnMouseTransition(object? sender, MouseEventArgs args)
    {
        lock (IsMouseDirectlyOverItemProperty)
        {
            if (_currentItem is not null)
            {
                DependencyObject oldItem = _currentItem;
                _currentItem = null;
                oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
            }
            IInputElement currentPosition = Mouse.DirectlyOver;
            if (currentPosition is not null)
            {
                RoutedEventArgs newItemArgs = new(UpdateOverItemEvent);
                currentPosition.RaiseEvent(newItemArgs);

            }
        }
    }
}
