using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BaseUISupport.Controls;

[TemplatePart(Name = "PART_HourEditor", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_MinuteEditor", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_SecondEditor", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_UpButton", Type = typeof(RepeatButton))]
[TemplatePart(Name = "PART_DownButton", Type = typeof(RepeatButton))]
public class TimePicker : Control, INotifyPropertyChanged
{
    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
    }
    public DateTime Date
    {
        get { return (DateTime)GetValue(DateProperty); }
        set
        {
            SetValue(DateProperty, value);
            OnPropertyChanged("Hours");
            OnPropertyChanged("Minutes");
            OnPropertyChanged("Seconds");
        }
    }

    public string Hours
    {
        get { return Date.ToString("HH"); }
        set
        {
            try
            {
                int t = 0;
                t = int.Parse(value);
                if (t > 0 && t < 24)
                {
                    DateTime d = new(Date.Year, Date.Month, Date.Day, t, Date.Minute, Date.Second);
                    SetValue(DateProperty, d);
                }
            }
            catch (Exception)
            {
            }
            OnPropertyChanged("Hours");
        }
    }

    public string Minutes
    {
        get { return Date.ToString("mm"); }
        set
        {
            try
            {
                int t = 0;
                t = int.Parse(value);
                if (t > 0 && t < 60)
                {
                    DateTime d = new(Date.Year, Date.Month, Date.Day, Date.Hour, t, Date.Second);
                    SetValue(DateProperty, d);
                }
            }
            catch (Exception)
            {
            }
            OnPropertyChanged("Minutes");
        }
    }
    public string Seconds
    {
        get { return Date.ToString("ss"); }
        set
        {
            try
            {
                int t = 0;
                t = int.Parse(value);
                if (t > 0 && t < 60)
                {
                    DateTime d = new(Date.Year, Date.Month, Date.Day, Date.Hour, Date.Minute, t);
                    SetValue(DateProperty, d);
                }
            }
            catch (Exception)
            {
            }
            OnPropertyChanged("Seconds");
        }
    }
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        upButton = Template.FindName("PART_UpButton", this) as RepeatButton;
        downButton = Template.FindName("PART_DownButton", this) as RepeatButton;
        hourEditor = Template.FindName("PART_HourEditor", this) as TextBox;
        minuteEditor = Template.FindName("PART_MinuteEditor", this) as TextBox;
        secondEditor = Template.FindName("PART_SecondEditor", this) as TextBox;

        if (upButton is null || downButton is null || hourEditor is null || minuteEditor is null || secondEditor is null) return;

        upButton.Click += RepeatButton_Click;
        downButton.Click += RepeatButton_Click;

        hourEditor.PreviewKeyDown += TextBox_KeyDown;
        minuteEditor.PreviewKeyDown += TextBox_KeyDown;
        secondEditor.PreviewKeyDown += TextBox_KeyDown;
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            Decrease();
            e.Handled = true;
        }
        if (e.Key == Key.Up)
        {
            Increase();
            e.Handled = true;
        }
    }

    private void Increase()
    {
        if (hourEditor is null || minuteEditor is null || secondEditor is null) return;
        if (hourEditor.IsFocused)
        {
            Date += TimeSpan.FromHours(1);
        }
        else if (minuteEditor.IsFocused)
        {
            Date += TimeSpan.FromMinutes(1);
        }
        else
        {
            if (!secondEditor.IsFocused) secondEditor.Focus();
            Date += TimeSpan.FromSeconds(1);
        }
    }

    private void Decrease()
    {
        if (hourEditor is null || minuteEditor is null || secondEditor is null) return;
        if (hourEditor.IsFocused)
        {
            Date -= TimeSpan.FromHours(1);
        }
        else if (minuteEditor.IsFocused)
        {
            Date -= TimeSpan.FromMinutes(1);
        }
        else
        {
            if (secondEditor.IsFocused) secondEditor.Focus();
            Date -= TimeSpan.FromSeconds(1);
        }
    }


    private void RepeatButton_Click(object? sender, RoutedEventArgs e)
    {
        if (e.Source == upButton)
        {
            Increase();
        }
        else
        {
            Decrease();
        }
    }

    private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TimePicker picker = (TimePicker)d;
        picker.OnPropertyChanged("Hours");
        picker.OnPropertyChanged("Minutes");
        picker.OnPropertyChanged("Seconds");
    }
    public static readonly DependencyProperty DateProperty =
    DependencyProperty.Register("Date", typeof(DateTime), typeof(TimePicker), new FrameworkPropertyMetadata(DateTime.Now, OnDateChanged));

    private RepeatButton? upButton, downButton;
    private TextBox? hourEditor, minuteEditor, secondEditor;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string property) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)); }

}
