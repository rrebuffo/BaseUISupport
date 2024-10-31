using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BaseUISupport.Helpers;

public class DataGridHelper
{
    static readonly Dictionary<DataGrid, Capture> Associations = [];

    public static bool GetFixCorners(DependencyObject obj)
    {
        return (bool)obj.GetValue(FixCornersProperty);
    }

    public static void SetFixCorners(DependencyObject obj, bool value)
    {
        obj.SetValue(FixCornersProperty, value);
    }

    public static readonly DependencyProperty FixCornersProperty = DependencyProperty.RegisterAttached("FixCorners", typeof(bool), typeof(DataGridHelper), new UIPropertyMetadata(false, OnFixCornersChanged));

    public static void OnFixCornersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid target) return;
        if (e.OldValue == e.NewValue) return;
        if ((bool)e.NewValue)
        {
            target.Loaded += DataGrid_Loaded;
            target.Unloaded += DataGrid_Unloaded;
        }
        else
        {
            target.Loaded -= DataGrid_Loaded;
            target.Unloaded -= DataGrid_Unloaded;
            if (Associations.ContainsKey(target)) Associations[target].Dispose();
        }
    }

    private static void Fix(DataGrid grid)
    {
        double? radius = null;
        for(var i=0; i<grid.Columns.Count; i++)
        {
            if (grid.FindChild<DataGridRowsPresenter>() is not DataGridRowsPresenter rows) continue;
            foreach(DataGridRow row in rows.Children)
            {
                if(row.FindChild<DataGridCellsPanel>() is DataGridCellsPanel cells)
                {
                    for (var c = 0; c < cells.Children.Count; c++)
                    {
                        if (cells.Children[c] is DataGridCell cell)
                        {
                            var is_first = c == 0;
                            var is_last = c == cells.Children.Count - 1;

                            if (VisualTreeHelpers.FindChild<Border>(cell, "CellBackground") is Border border)
                            {
                                radius ??= border.CornerRadius.TopLeft;

                                if (is_first) border.CornerRadius = new CornerRadius(radius.Value, 0, 0, radius.Value);
                                else if (is_last) border.CornerRadius = new CornerRadius(0, radius.Value, radius.Value, 0);
                                else border.CornerRadius = new CornerRadius(0);

                            }
                        }

                    }
                }
            }
        }
    }

    static void DataGrid_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not DataGrid target) return;
        if (Associations.ContainsKey(target)) Associations[target].Dispose();
    }

    static void DataGrid_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not DataGrid target) return;
        Associations[target] = new Capture(target);
        Fix(target);
    }

    class Capture : IDisposable
    {
        private readonly DataGrid DataGrid;

        public Capture(DataGrid target)
        {
            DataGrid = target;
            DataGrid.LoadingRow += DataGrid_LoadingRow;
            DataGrid.Initialized += DataGrid_Initialized;
            DataGrid.InitializingNewItem += DataGrid_InitializingNewItem;
        }

        private void DataGrid_Initialized(object? sender, EventArgs e)
        {
            Fix(DataGrid);
        }

        private void DataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            Fix(DataGrid);
        }

        private void DataGrid_InitializingNewItem(object? sender, InitializingNewItemEventArgs e)
        {
            Fix(DataGrid);
        }

        public void Dispose()
        {
            DataGrid.LoadingRow -= DataGrid_LoadingRow;
            DataGrid.Initialized -= DataGrid_Initialized;
            DataGrid.InitializingNewItem -= DataGrid_InitializingNewItem;
        }
    }
}

public class EnterKeyTraversal
{
    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    static void FrameworkElement_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement element) return;

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
        }
    }

    private static void FrameworkElement_Unloaded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement element) return;
        element.Unloaded -= FrameworkElement_Unloaded;
        element.PreviewKeyDown -= FrameworkElement_PreviewKeyDown;
    }

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(EnterKeyTraversal), new UIPropertyMetadata(false, IsEnabledChanged));

    static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || e.NewValue is not bool value) return;

        if (value)
        {
            element.Unloaded += FrameworkElement_Unloaded;
            element.PreviewKeyDown += FrameworkElement_PreviewKeyDown;
        }
        else
        {
            element.PreviewKeyDown -= FrameworkElement_PreviewKeyDown;
        }
    }
}