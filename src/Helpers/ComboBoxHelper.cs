using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BaseUISupport.Helpers;

class ComboBoxHelper
{
    public static readonly DependencyProperty FixComboBoxDirectionProperty = DependencyProperty.RegisterAttached("FixComboBoxDirection", typeof(bool), typeof(ComboBoxHelper), new UIPropertyMetadata(false, OnFixComboBoxDirectionChanged));
    public static bool GetFixComboBoxDirection(DependencyObject obj) { return (bool)obj.GetValue(FixComboBoxDirectionProperty); }
    public static void SetFixComboBoxDirection(DependencyObject obj, bool value) { obj.SetValue(FixComboBoxDirectionProperty, value); }

    private static readonly Dictionary<ComboBox, Tracker> Tracked = [];

    public static void OnFixComboBoxDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox target) return;
        if (e.OldValue == e.NewValue) return;
        if ((bool)e.NewValue)
        {
            target.Loaded += ComboBox_Loaded;
            target.Unloaded += ComboBox_Unloaded;
        }
        else
        {
            target.Loaded -= ComboBox_Loaded;
            target.Unloaded -= ComboBox_Unloaded;
        }
    }

    private static void ComboBox_Loaded(object? sender, RoutedEventArgs e)
    {
        if(sender is ComboBox target && !Tracked.ContainsKey(target)) Tracked.Add(target, new Tracker(target));
    }

    private static void ComboBox_Unloaded(object? sender, RoutedEventArgs e)
    {
        if(sender is ComboBox target && Tracked.ContainsKey(target) && Tracked[target] is Tracker tracker)
        {
            tracker.Dispose();
            Tracked.Remove(target);
        }
    }

    private class Tracker : IDisposable
    {
        private readonly ComboBox ComboBox;
        private readonly Popup? DropDown;
        private readonly CornerRadius ComboRadius;
        
        public Tracker(ComboBox comboBox)
        {
            ComboBox = comboBox;
            if (comboBox.FindChild<Popup>() is Popup dropDown)
            {
                DropDown = dropDown;
                DropDown.Opened += DropDown_Opened;
            }
            else return;
            if (ComboBox.FindChild<ToggleButton>() is ToggleButton toggle && toggle.FindChild<Border>() is Border toggle_border)
            {
                ComboRadius = toggle_border.CornerRadius;
            }
        }

        private void DropDown_Opened(object? sender, EventArgs e)
        {
            if (DropDown is null) return;
            DropDown.Closed += DropDown_Closed;
            SetCorners();
        }

        private void DropDown_Closed(object? sender, EventArgs e)
        {
            if (DropDown is null) return;
            DropDown.Closed -= DropDown_Closed;

            if (ComboBox.FindChild<ToggleButton>() is ToggleButton toggle && toggle.FindChild<Border>() is Border toggle_border)
            {
                toggle_border.CornerRadius = ComboRadius;
            }
        }

        public void SetCorners()
        {
            if (DropDown is null) return;
            if (ComboBox.FindParent<Window>() is not Window window) return;
            DropDown.Closed += DropDown_Closed;
            Point popup = DropDown.Child.PointToScreen(ComboBox.PointFromScreen(default));
            var radius = ComboRadius.TopLeft;

            var cb_width = ComboBox.ActualWidth;
            var cb_height = ComboBox.ActualHeight;
            var dd_width = DropDown.Child.RenderSize.Width;
            var dd_height = DropDown.Child.RenderSize.Height;
            var dd_x = popup.X;
            var dd_y = popup.Y;

            var combo_tl = (dd_y < 0) ? 0 : radius;
            var combo_tr = (dd_y < 0) ? 0 : radius;
            var combo_br = (dd_y > 0) ? 0 : radius;
            var combo_bl = (dd_y > 0) ? 0 : radius;
            var combo_corner = new CornerRadius(combo_tl, combo_tr, combo_br, combo_bl);

            var popup_tl = (dd_x == 0 && dd_y > 0) ? 0 : radius;
            var popup_tr = ((dd_width == cb_width || dd_x < 0 - dd_width - cb_width) && dd_y > 0) ? 0 : radius;
            var popup_br = ((dd_width == cb_width || dd_x < 0 - dd_width - cb_width) && dd_y < 0) ? 0 : radius;
            var popup_bl = (dd_x == 0 && dd_y < 0) ? 0 : radius;
            var popup_corner = new CornerRadius(popup_tl, popup_tr, popup_br, popup_bl);

            if (VisualTreeHelpers.FindChild<Border>(DropDown.Child) is Border popup_border1 && popup_border1.FindChild<Border>() is Border popup_border2)
            {
                popup_border1.CornerRadius = popup_border2.CornerRadius = popup_corner;
            }
            if (ComboBox.FindChild<ToggleButton>() is ToggleButton toggle && toggle.FindChild<Border>() is Border toggle_border)
            {
                toggle_border.CornerRadius = combo_corner;
            }
        }

        public void Dispose()
        {
            try
            {
                if (DropDown is not null)
                {
                    DropDown.Opened -= DropDown_Opened;
                    DropDown.Closed -= DropDown_Closed;
                }
            }
            catch (Exception) { }
        }
    }

}
