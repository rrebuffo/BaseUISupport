using System.Windows;
using System.Windows.Media;

namespace BaseUISupport.Helpers;

public static class VisualTreeHelpers
{
    public static T? FindParent<T>(this DependencyObject current) where T : DependencyObject
    {
        current = VisualTreeHelper.GetParent(current);

        while (current is not null)
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        };
        return null;
    }

    public static T? FindParent<T>(this DependencyObject current, T lookupItem) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T t && current == lookupItem) return t;
            current = VisualTreeHelper.GetParent(current);
        };
        return null;
    }

    public static T? FindParent<T>(this DependencyObject current, string? parentName) where T : DependencyObject
    {
        while (current is not null)
        {
            if (!string.IsNullOrEmpty(parentName))
            {
                if (current is T t && current is FrameworkElement element && element.Name == parentName) return t;
            }
            else if (current is T t)
            {
                return t;
            }
            current = VisualTreeHelper.GetParent(current);
        };

        return null;

    }

    public static T? FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent is null) return null;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement element)
            {
                if (child is T t && element.Name == childName) return t; 
                else if (FindChild<T>(child, childName) is T r) return r;
            }
        }

        return null;
    }

    public static T? FindChild<T>(this DependencyObject parent) where T : DependencyObject
    {
        if (parent is null) return null;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;
            else if (FindChild<T>(child) is T r) return r;
        }
        return null;
    }
}
