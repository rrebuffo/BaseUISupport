using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BaseUISupport.Controls;

/// <summary>
/// Interaction logic for StatusLabel.xaml
/// </summary>
public partial class StatusLabel : UserControl
{
    public StatusLabel()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(DependencyObject), typeof(StatusLabel));
    public DependencyObject Label
    {
        get
        {
            return (DependencyObject)GetValue(LabelProperty);
        }
        set
        {
            SetValue(LabelProperty, value);
        }
    }

    public static readonly DependencyProperty LabelStateProperty = DependencyProperty.Register(nameof(LabelState), typeof(StatusLabelState), typeof(StatusLabel), new PropertyMetadata(StatusLabelState.Ok));
    public StatusLabelState LabelState
    {
        get
        {
            return (StatusLabelState)GetValue(LabelStateProperty);
        }
        set
        {
            SetValue(LabelStateProperty, value);
        }
    }

    public static readonly DependencyProperty LabelProgressProperty = DependencyProperty.Register(nameof(LabelProgress), typeof(double), typeof(StatusLabel), new PropertyMetadata(0d));
    public double LabelProgress
    {
        get
        {
            return (double)GetValue(LabelProgressProperty);
        }
        set
        {
            SetValue(LabelProgressProperty, value);
        }
    }

    public static readonly DependencyProperty LabelIconProperty = DependencyProperty.Register(nameof(LabelIcon), typeof(Geometry), typeof(StatusLabel), new PropertyMetadata(null));
    public Geometry LabelIcon
    {
        get
        {
            return (Geometry)GetValue(LabelIconProperty);
        }
        set
        {
            SetValue(LabelIconProperty, value);
        }
    }
}

public enum StatusLabelState
{
    Idle,
    Ok,
    Warning,
    Error,
}
