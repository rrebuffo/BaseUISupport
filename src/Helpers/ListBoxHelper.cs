using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Adorner = System.Windows.Documents.Adorner;

namespace BaseUISupport.Helpers;

public class ListBoxHelper
{
    private const int DefaultScrollTolerance = 50;
    private const int DefaultScrollOffset = 20;
    private static readonly Dictionary<ListBox, ListBoxHandler> Tracking = new();

    public static int ScrollTolerance { get; set; } = DefaultScrollTolerance;
    public static int ScrollOffset { get; set; } = DefaultScrollOffset;

    class ListBoxHandler : IDisposable
    {
        private readonly ListBox? TargetListBox;
        private INotifyCollectionChanged? Observable;
        private readonly Dictionary<object, ListBoxItem> Tracked = new();

        private bool _initialized = false;

        public bool ScrollOnNewItem = false;
        public bool DragAndDrop = false;
        public bool SelectionRectangle = false;

        public ListBoxHandler(ListBox? target)
        {
            if (target is null) return;
            TargetListBox = target;
            TypeDescriptor.GetProperties(target)["ItemsSource"]?.AddValueChanged(target, ListBox_ItemsSourceChanged);
            Observable = TargetListBox.ItemsSource as INotifyCollectionChanged;
            if (Observable is null) return;
            Observable.CollectionChanged += Collection_CollectionChanged;
        }

        private void ListBox_ItemsSourceChanged(object? sender, EventArgs e)
        {
            Clear();
            InitItems();
            if (TargetListBox is null) return;
            TargetListBox.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            if (Observable is not INotifyCollectionChanged obs) return;
            Observable = obs;
            Observable.CollectionChanged += Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (TargetListBox is null) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (DragAndDrop && e.NewItems is not null)
                    {
                        foreach (object newItem in e.NewItems)
                        {
                            if (TargetListBox.ItemContainerGenerator.ContainerFromItem(newItem) is ListBoxItem item)
                            {
                                item.PreviewMouseLeftButtonDown -= ItemPress;
                                item.PreviewMouseLeftButtonDown += ItemPress;
                                if (!Tracked.ContainsKey(newItem)) Tracked.Add(newItem, item);
                            }
                        }
                    }
                    if (ScrollOnNewItem && e.NewItems is not null)
                    {
                        if (e.NewItems[0] is object n)
                        {
                            TargetListBox.ScrollIntoView(n);
                            TargetListBox.SelectedItem = n;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (DragAndDrop && e.OldItems is not null)
                    {
                        foreach (object oldItem in e.OldItems)
                        {
                            if (Tracked.ContainsKey(oldItem))
                            {
                                Tracked[oldItem].PreviewMouseLeftButtonDown -= ItemPress;
                                Tracked.Remove(oldItem);
                            }
                        }
                    }
                    break;
            }
        }

        private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            InitItems();
            
        }

        public void Init()
        {
            if (_initialized || TargetListBox is null || TargetListBox.ItemsSource is null) return;
            InitItems();
            TargetListBox.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
        }

        private void InitItems()
        {
            if (TargetListBox is null || TargetListBox.ItemsSource is null) return;
            foreach (object item in TargetListBox.ItemsSource)
            {
                if (TargetListBox.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem listItem)
                {
                    listItem.PreviewMouseLeftButtonDown -= ItemPress;
                    listItem.PreviewMouseLeftButtonDown += ItemPress;
                    if (!Tracked.ContainsKey(item))
                    {
                        Tracked.Add(item, listItem);
                    }
                }
            }
        }

        private void Clear()
        {
            if (Observable != null) Observable.CollectionChanged -= Collection_CollectionChanged;
            if (TargetListBox is not null) TargetListBox.ItemContainerGenerator.ItemsChanged -= ItemContainerGenerator_ItemsChanged;
            foreach (ListBoxItem item in Tracked.Values) if (item != null) item.PreviewMouseLeftButtonDown -= ItemPress;
        }

        public void Dispose()
        {
            Clear();
        }
    }



    #region SCROLL ON NEW ITEM

    public static readonly DependencyProperty ScrollOnNewItemProperty = DependencyProperty.RegisterAttached("ScrollOnNewItem", typeof(bool), typeof(ListBoxHelper), new UIPropertyMetadata(false, OnScrollOnNewItemChanged));
    public static bool GetScrollOnNewItem(DependencyObject obj) { return (bool)obj.GetValue(ScrollOnNewItemProperty); }
    public static void SetScrollOnNewItem(DependencyObject obj, bool value) { obj.SetValue(ScrollOnNewItemProperty, value); }

    public static void OnScrollOnNewItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox target) return;
        if ((bool)e.NewValue)
        {
            target.Loaded -= ScrollListBox_Loaded;
            target.Loaded += ScrollListBox_Loaded;
        }
        else
        {
            target.Loaded -= ScrollListBox_Loaded;
        }
    }

    private static void ScrollListBox_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ListBox target) return;
        e.Handled = false;
        SetupScrollHandler(target);
    }

    private static void ScrollListBox_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ListBox target) return;
        if (Tracking.ContainsKey(target))
        {
            Tracking[target].Dispose();
            Tracking.Remove(target);
        }
    }

    private static void SetupScrollHandler(ListBox target)
    {
        ListBoxHandler handler = Tracking.ContainsKey(target) ? Tracking[target] : new ListBoxHandler(target);
        handler.ScrollOnNewItem = true;
        if (!Tracking.ContainsKey(target))
        {
            Tracking.Add(target, handler);
        }
        handler.Init();
    }

    #endregion

    #region DRAG AND DROP

    public static readonly DependencyProperty EnableDragAndDropProperty = DependencyProperty.RegisterAttached("EnableDragAndDrop", typeof(bool), typeof(ListBoxHelper), new UIPropertyMetadata(false, OnEnableDragAndDropChanged));
    public static bool GetEnableDragAndDrop(DependencyObject obj) { return (bool)obj.GetValue(EnableDragAndDropProperty); }
    public static void SetEnableDragAndDrop(DependencyObject obj, bool value) { obj.SetValue(EnableDragAndDropProperty, value); }

    public static void OnEnableDragAndDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox target) return;
        if ((bool)e.NewValue)
        {
            target.Loaded -= DragDropListBox_Loaded;
            target.Loaded += DragDropListBox_Loaded;
        }
        else
        {
            target.Loaded -= DragDropListBox_Loaded;
        }
    }

    private static readonly List<object> DraggedItems = new();
    private static Point StartPoint;
    private static bool Drag = false;
    private static bool ReleaseSelection = false;
    private static SelectionMode LastSelectionMode = SelectionMode.Single;
    private static ParentPanelOrientation PanelOrientation = ParentPanelOrientation.Vertical;
    private static readonly double DragThreshold = 10;
    private static int DropIndex = 0;
    private static ListBox? List = null;
    private static ScrollViewer? Scroll = null;
    private static ListBoxItem? Item = null;
    private static Panel? Container = null;

    private static void DragDropListBox_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ListBox target) return;
        e.Handled = false;
        SetupDragDropHandler(target);
    }

    private static void SetupDragDropHandler(ListBox target)
    {
        if (target is not null)
        {
            ListBoxHandler handler = Tracking.ContainsKey(target) ? Tracking[target] : new ListBoxHandler(target);
            handler.DragAndDrop = true;
            if (!Tracking.ContainsKey(target))
            {
                Tracking.Add(target, handler);
            }
            handler.Init();
        }
    }

    private static void DragDropListBox_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ListBox target) return;
        if (Tracking.ContainsKey(target))
        {
            var toRemove = Tracking[target];
            Tracking.Remove(target);
            toRemove.Dispose();
        }
    }

    private enum SelectionMode
    {
        Single,
        Range,
        Items
    }

    private static void ItemPress(object? sender, MouseButtonEventArgs e)
    {
        Drag = false;
        if (sender is ListBoxItem item && item.FindParent<ListBox>() is ListBox list)
        {
            Item = item;
            List = list;
            if (List.SelectedItems.Count > 1)
            {
                if (Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (List.SelectedItems.Contains(Item.DataContext))
                    {
                        ReleaseSelection = true;
                        e.Handled = true;
                    }
                    else
                    {
                        List.SelectedItems.Clear();
                        List.SelectedItems.Add(Item.DataContext);
                        LastSelectionMode = SelectionMode.Single;
                    }
                }
            }
            else
            {
                if (Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (list.SelectionMode == System.Windows.Controls.SelectionMode.Single)
                    {
                        List.SelectedItem = Item.DataContext;
                    }
                    else
                    {
                        List.SelectedItems.Clear();
                        List.SelectedItems.Add(Item.DataContext);
                    }
                }
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    LastSelectionMode = SelectionMode.Range;
                }
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    LastSelectionMode = SelectionMode.Items;
                }
            }
            DraggedItems.Clear();
            if (List.SelectionMode == System.Windows.Controls.SelectionMode.Single) DraggedItems.Add(item.DataContext);
            else foreach (object obj in List.SelectedItems) DraggedItems.Add(obj);
            if (list.FindChild<ScrollViewer>() is ScrollViewer scroll)
            {
                Scroll = scroll;
            }
            StartPoint = e.GetPosition(List);
            List.PreviewMouseMove += PreventSelectionChange;
            if (List.FindParent<Window>() is Window window)
            {
                window.PreviewMouseUp += Window_PreviewMouseUp;
                window.PreviewMouseMove += HandleDrag;
            }
        }
    }

    private static void ShowDropRegion(object? sender, DragEventArgs e)
    {
        if (Scroll is null || Container is null) return;
        Point MousePosition = e.GetPosition(Scroll);

        if (Scroll.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
        {
            if (MousePosition.X < ScrollTolerance) Scroll.ScrollToHorizontalOffset(Scroll.HorizontalOffset - (ScrollOffset * ((ScrollTolerance - MousePosition.X) / ScrollTolerance)));
            else if (MousePosition.X > Scroll.ActualWidth - ScrollTolerance) Scroll.ScrollToHorizontalOffset(Scroll.HorizontalOffset + (ScrollOffset * (ScrollTolerance - (Scroll.ActualWidth - MousePosition.X)) / ScrollTolerance));
        }
        if (Scroll.ComputedVerticalScrollBarVisibility == Visibility.Visible)
        {
            if (MousePosition.Y < ScrollTolerance) Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - (ScrollOffset * (ScrollTolerance - MousePosition.Y) / ScrollTolerance));
            else if (MousePosition.Y > Scroll.ActualHeight - ScrollTolerance) Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + (ScrollOffset * (ScrollTolerance - (Scroll.ActualHeight - MousePosition.Y)) / ScrollTolerance));
        }

        int Closest = -1;
        double Distance = -1;
        foreach (ListBoxItem item in Container.Children)
        {
            Point bounds = item.TranslatePoint(new Point(-Scroll.HorizontalOffset, -Scroll.VerticalOffset), Container);
            double x = Math.Abs(bounds.X + item.RenderSize.Width / 2 - MousePosition.X);
            double y = Math.Abs(bounds.Y + item.RenderSize.Height / 2 - MousePosition.Y);
            double distance = (x + y) / 2;
            if (Closest < 0 || Distance > distance)
            {
                Distance = distance;
                Closest = Container.Children.IndexOf(item);
            }
        }
        if (Closest >= 0)
        {
            switch (PanelOrientation)
            {
                case ParentPanelOrientation.Horizontal:
                    if (Container.Children[Closest] is ListBoxItem v_item)
                    {
                        Point bounds = v_item.TranslatePoint(new Point(-Scroll.HorizontalOffset, 0), Container);
                        double x = bounds.X + v_item.RenderSize.Width / 2 - MousePosition.X;
                        DropIndex = x < 0 ? Closest + 1 : Closest;
                    }
                    break;
                case ParentPanelOrientation.Vertical:
                    if (Container.Children[Closest] is ListBoxItem h_item)
                    {
                        Point bounds = h_item.TranslatePoint(new Point(0, -Scroll.VerticalOffset), Container);
                        double y = bounds.Y + h_item.RenderSize.Height / 2 - MousePosition.Y;
                        DropIndex = y < 0 ? Closest + 1 : Closest;
                    }
                    break;
            }
        }
        else Closest = DropIndex = Container.Children.Count;

        if (Closest < DropIndex && Closest < Container.Children.Count - 1) Closest++;
        ListBoxItem closest = (ListBoxItem)Container.Children[Closest];
        AdornerLayer layer = AdornerLayer.GetAdornerLayer(List);
        ReleaseAdorners();
        MarkDraggedItems();
        DragAdorner drag = new(closest, PanelOrientation, Closest < DropIndex);
        layer.Add(drag);
    }

    private static void MarkDraggedItems(double opacity = .5)
    {
        if (Container is null) return;
        foreach (ListBoxItem item in Container.Children)
        {
            item.IsHitTestVisible = false;
            if (DraggedItems.Contains(item.DataContext)) item.Opacity = opacity;
        }
    }

    private static void HideDropRegion(object sender, DragEventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ReleaseAdorners();
            MarkDraggedItems(1);
        }, DispatcherPriority.Background);
    }

    private static void PreventSelectionChange(object? sender, MouseEventArgs e)
    {
        if (List is null) return;
        List.ReleaseMouseCapture();
    }

    private static void SetOrientation()
    {
        ParentPanelOrientation orientation = ParentPanelOrientation.Vertical;
        if (VisualTreeHelper.GetParent(Item) is StackPanel stack)
        {
            orientation = stack.Orientation switch
            {
                Orientation.Horizontal => ParentPanelOrientation.Horizontal,
                _ => ParentPanelOrientation.Vertical,
            };
            Container = stack;
        }
        if (VisualTreeHelper.GetParent(Item) is WrapPanel wrap)
        {
            orientation = wrap.Orientation switch
            {
                Orientation.Horizontal => ParentPanelOrientation.Horizontal,
                _ => ParentPanelOrientation.Vertical,
            };
            Container = wrap;
        }
        PanelOrientation = orientation;

    }

    private static void ReleaseAdorners()
    {
        if (Container is not Panel p || List is not ListBox list) return;
        try
        {
            foreach (ListBoxItem item in p.Children)
            {
                if (AdornerLayer.GetAdornerLayer(list) is AdornerLayer layer)
                {
                    if (layer.GetAdorners(item) is Adorner[] adorners)
                    {
                        foreach (Adorner adorner in adorners) layer.Remove(adorner);
                    }
                }
                item.IsHitTestVisible = true;
            }
        }
        catch { }
    }

    private static void HandleDrag(object? sender, MouseEventArgs e)
    {
        if (sender is not Window window) return;
        if (!DraggedItems.Any())
        {
            Drag = false;
            window.MouseMove -= HandleDrag;
            return;
        }
        if (!Drag)
        {
            var delta = Math.Abs(e.GetPosition(List).X - StartPoint.X) > DragThreshold || Math.Abs(e.GetPosition(List).Y - StartPoint.Y) > DragThreshold;
            if (delta)
            {
                Drag = true;
                window.AllowDrop = false;
                window.PreviewMouseMove -= HandleDrag;
                SetOrientation();
                if (Container is null || List is null)
                {
                    Drop(false);
                    return;
                }
                
                List.PreviewDragOver += ShowDropRegion;
                List.PreviewDragLeave += HideDropRegion;
                List.PreviewDrop += List_PreviewDrop;
                DragDropEffects result = DragDrop.DoDragDrop(List, DraggedItems, DragDropEffects.Move);
                if (result == DragDropEffects.None)
                {
                    Drop(false);
                    return;
                }
                if (List.SelectionMode == System.Windows.Controls.SelectionMode.Single) List.SelectedItem = DraggedItems.FirstOrDefault();
                else
                {
                    List.SelectedItems.Clear();
                    foreach (object item in DraggedItems) List.SelectedItems.Add(item);
                }
                if (ReleaseSelection) ReleaseSelection = false;

            }
        }
    }

    private static void Window_PreviewMouseUp(object? sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        Drop(false);
    }

    private static void ReleaseWindow(object? sender)
    {
        if (sender is not Window window) return;
        window.PreviewMouseMove -= HandleDrag;
        window.PreviewMouseUp -= Window_PreviewMouseUp;
        window.AllowDrop = false;
    }

    private static void List_PreviewDrop(object? sender, DragEventArgs e)
    {
        Drop(true);
        //e.Handled = true;
    }

    private static void Drop(bool success = false)
    {
        if (List is null) return;
        if (ReleaseSelection) Release();
        List.PreviewMouseMove -= PreventSelectionChange;
        List.PreviewDragOver -= ShowDropRegion;
        List.PreviewDrop -= List_PreviewDrop;
        if (List.FindParent<Window>() is Window window) ReleaseWindow(window);
        if (Container is not null)
        {
            foreach (ListBoxItem item in Container.Children)
            {
                if (DraggedItems.Contains(item.DataContext)) item.Opacity = 1;
            }
            if (success) DropItems();
            ReleaseAdorners();
        }
        if (ReleaseSelection) Release();
        DraggedItems.Clear();
        Drag = false;
    }

    private static void Release()
    {
        if (List is null || Item is null) return;
        List.UnselectAll();
        List.SelectedItem = Item.DataContext;
        ReleaseSelection = false;
        LastSelectionMode = SelectionMode.Single;
    }

    private static void DropItems()
    {
        if (List is null) return;
        List<object> ItemsToDrop = new();
        switch (LastSelectionMode)
        {
            case SelectionMode.Single:
            case SelectionMode.Items:
                ItemsToDrop = DraggedItems.ToList();
                break;
            case SelectionMode.Range:
                foreach (object item in List.Items) if (DraggedItems.Contains(item)) ItemsToDrop.Add(item);
                break;
        }
        AddItems(ItemsToDrop);
        Drag = false;
    }

    private static void AddItems(List<object> Items)
    {
        var offset = 0;
        var index = DropIndex;
        if (List is not ListBox list || list.ItemsSource is not IList collection) return;
        foreach (object item in Items)
        {
            if (list.Items.IndexOf(item) < index) index--;
            var targetindex = index + offset;
            collection.Remove(item);
            collection.Insert(targetindex, item);
            offset++;
        }
    }

    #endregion

    #region SELECTION RECTANGLE

    public static readonly DependencyProperty EnableSelectionRectangleProperty = DependencyProperty.RegisterAttached("EnableSelectionRectangle", typeof(bool), typeof(ListBoxHelper), new UIPropertyMetadata(false, OnEnableSelectionRectangleChanged));
    public static bool GetEnableSelectionRectangle(DependencyObject obj) { return (bool)obj.GetValue(EnableSelectionRectangleProperty); }
    public static void SetEnableSelectionRectangle(DependencyObject obj, bool value) { obj.SetValue(EnableSelectionRectangleProperty, value); }

    public static void OnEnableSelectionRectangleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox target) return;
        if ((bool)e.NewValue)
        {
            target.Loaded -= SelectionListBox_Loaded;
            target.Loaded += SelectionListBox_Loaded;
        }
        else
        {
            target.Loaded -= SelectionListBox_Loaded;
        }
    }
    private static Panel? GetListBoxPanel(ListBox target)
    {
        if (target.FindChild<ScrollViewer>() is ScrollViewer scroll
            && scroll.FindChild<ScrollContentPresenter>() is ScrollContentPresenter content
            && content.FindChild<Panel>() is Panel container)
        {
            return container;
        }
        return null;
    }

    private static void SelectionListBox_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ListBox target || GetListBoxPanel(target) is not Panel container) return;
        
        container.MouseLeftButtonDown += SelectionPanel_MouseLeftButtonDown;
        if (container.Background is null) container.Background = Brushes.Transparent;
        container.IsHitTestVisible = true;
    }

    private static void SelectionListBox_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ListBox target || GetListBoxPanel(target) is not Panel container) return;
        target.Unloaded -= SelectionListBox_Unloaded;
        container.MouseLeftButtonDown -= SelectionPanel_MouseLeftButtonDown;
    }

    private static void SelectionPanel_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not Panel container) return;
        if (container.FindParent<ListBox>() is ListBox target && target.SelectionMode != System.Windows.Controls.SelectionMode.Single)
        {
            SelectionPanel = container;
            SelectionListBox = target;
            SelectionPanel.CaptureMouse();
            SelectionStart = e.GetPosition(SelectionPanel);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(SelectionPanel);
            if (Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Shift)
                foreach (object selected in SelectionListBox.SelectedItems) SelectionCache.Add(selected);
            else
                SelectionListBox.UnselectAll();
            if (target.FindParent<Window>() is Window window)
            {
                window.PreviewMouseLeftButtonUp += Window_PreviewMouseLeftButtonUp;
            }
            SelectionPanel.PreviewMouseMove += SelectionPanel_PreviewMouseMove;
        }
    }

    private static void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Window window) window.PreviewMouseLeftButtonUp -= Window_PreviewMouseLeftButtonUp;
        if (SelectionPanel is null || SelectionListBox is null) return;
        SelectionPanel.PreviewMouseMove -= SelectionPanel_PreviewMouseMove;
        if (Keyboard.Modifiers == ModifierKeys.None && !SelectionItems.Any()) SelectionListBox.UnselectAll();
        AdornerLayer layer = AdornerLayer.GetAdornerLayer(SelectionPanel);
        if (SelectionAdornment != null && layer.GetAdorners(SelectionPanel).Contains(SelectionAdornment))
        {
            layer.Remove(SelectionAdornment);
            SelectionAdornment = null;
        }
        SelectionItems.Clear();
        SelectionCache.Clear();
        SelectionPanel.ReleaseMouseCapture();
        SelectionListBox = null;
        SelectionPanel = null;
    }

    private static void SelectionPanel_PreviewMouseMove(object? sender, MouseEventArgs e)
    {
        if (sender is not Panel panel || SelectionStart is not Point start || panel.FindParent<ScrollViewer>() is not ScrollViewer scroll) return;
        Point MousePosition = e.GetPosition(scroll);
        if (scroll.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
        {
            if (MousePosition.X < ScrollTolerance) scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset - (ScrollOffset / 10 * ((ScrollTolerance - MousePosition.X) / ScrollTolerance)));
            else if (MousePosition.X > scroll.ActualWidth - ScrollTolerance) scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + (ScrollOffset / 10 * (ScrollTolerance - (scroll.ActualWidth - MousePosition.X)) / ScrollTolerance));
        }
        if (scroll.ComputedVerticalScrollBarVisibility == Visibility.Visible)
        {
            if (MousePosition.Y < ScrollTolerance) scroll.ScrollToVerticalOffset(scroll.VerticalOffset - (ScrollOffset / 10 * (ScrollTolerance - MousePosition.Y) / ScrollTolerance));
            else if (MousePosition.Y > scroll.ActualHeight - ScrollTolerance) scroll.ScrollToVerticalOffset(scroll.VerticalOffset + (ScrollOffset / 10 * (ScrollTolerance - (scroll.ActualHeight - MousePosition.Y)) / ScrollTolerance));
        }
        Rect selection = new(start, e.GetPosition(SelectionPanel));
        if (SelectionPanel is not null && SelectionAdornment is null && (selection.Width + selection.Height) / 2 > SelectionThreshold)
        {
            SelectionAdornment = new SelectionAdorner(SelectionPanel, selection);
            AdornerLayer.GetAdornerLayer(SelectionPanel).Add(SelectionAdornment);
        }
        if (SelectionAdornment is not null && SelectionListBox is not null)
        {
            bool add = Keyboard.Modifiers == ModifierKeys.Shift;
            bool inv = Keyboard.Modifiers == ModifierKeys.Control;
            SelectionAdornment.Rect = selection;
            SelectionAdornment.InvalidateVisual();
            SelectionItems.Clear();
            VisualTreeHelper.HitTest(SelectionPanel, new HitTestFilterCallback(ListBoxItemFilter), new HitTestResultCallback(RectangleHitTest), new GeometryHitTestParameters(new RectangleGeometry(selection)));
            SelectionListBox.SelectedItems.Clear();
            foreach (object cached in SelectionCache)
            {
                if (SelectionItems.Contains(cached) && inv) SelectionItems.Remove(cached);
                else SelectionItems.Add(cached);
            }
            foreach (object item in SelectionItems) SelectionListBox.SelectedItems.Add(item);
        }
    }

    private static HitTestResultBehavior RectangleHitTest(HitTestResult result)
    {
        return HitTestResultBehavior.Continue;
    }

    private static HitTestFilterBehavior ListBoxItemFilter(DependencyObject o)
    {
        if (o is ListBoxItem item)
        {
            SelectionItems.Add(item.DataContext);
            return HitTestFilterBehavior.ContinueSkipChildren;
        }
        else
        {
            return HitTestFilterBehavior.ContinueSkipSelf;
        }
    }

    private static readonly int SelectionThreshold = 3;
    private static ListBox? SelectionListBox;
    private static Panel? SelectionPanel;
    private static Point? SelectionStart;
    private static SelectionAdorner? SelectionAdornment;
    private static readonly List<object> SelectionCache = new();
    private static readonly List<object> SelectionItems = new();


    #endregion

    private class DragAdorner : Adorner
    {
        public DragAdorner(UIElement adornedElement, ParentPanelOrientation orientation = ParentPanelOrientation.Vertical, bool showAtEnd = false) : base(adornedElement)
        {
            Rect = new Rect(new Point(0, 0), adornedElement.RenderSize);
            Orientation = orientation;
            ShowAtEnd = showAtEnd;
            if (adornedElement.GetValue(MarginProperty) is Thickness margin)
            {
                Thickness = orientation == ParentPanelOrientation.Horizontal ? margin.Left + margin.Right : margin.Top + margin.Bottom;
            }
            IsHitTestVisible = false;
        }

        public double Thickness = 0;
        public Rect Rect;
        public ParentPanelOrientation Orientation = ParentPanelOrientation.Vertical;
        public bool ShowAtEnd = false;

        protected override void OnRender(DrawingContext context)
        {
            Rect size;
            switch (Orientation)
            {
                case ParentPanelOrientation.Horizontal:
                    var offset_x = ShowAtEnd ? Rect.Width : -Thickness;
                    size = new Rect(Rect.X + offset_x, Rect.Y, Thickness, Rect.Height);
                    break;
                case ParentPanelOrientation.Vertical:
                    var offset_y = ShowAtEnd ? Rect.Height : -Thickness;
                    size = new Rect(Rect.X, Rect.Y + offset_y, Rect.Width, Thickness);
                    break;
            }
            SolidColorBrush brush = (SolidColorBrush)FindResource("MainAccent") ?? Brushes.White;
            context.DrawRoundedRectangle(brush, null, size, Thickness/2, Thickness/2);
        }
    }

    private class SelectionAdorner : Adorner
    {
        public SelectionAdorner(UIElement adornedElement, Rect rect) : base(adornedElement)
        {
            Rect = rect;
            IsHitTestVisible = false;
            SnapsToDevicePixels = true;
        }

        public double Thickness = 1;
        public Rect Rect;

        protected override void OnRender(DrawingContext context)
        {
            SolidColorBrush accent = (SolidColorBrush)FindResource("MainAccent") ?? Brushes.White;
            SolidColorBrush brush = new(accent is not null ? accent.Color : Colors.White)
            {
                Opacity = .6
            };
            SolidColorBrush stroke = (SolidColorBrush)FindResource("MainAccent") ?? Brushes.White;
            context.DrawRectangle(brush, new Pen(stroke, Thickness), Rect);
        }
    }

    private enum ParentPanelOrientation
    {
        Vertical,
        Horizontal
    }
}
