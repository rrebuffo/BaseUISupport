using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaseUISupport.Controls;

[TemplatePart(Name = "PART_IconPath", Type = typeof(Path))]
[TemplatePart(Name = "PART_Label", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_Border", Type = typeof(Border))]

public class StatusLabel : Control
{
    static StatusLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusLabel), new FrameworkPropertyMetadata(typeof(StatusLabel)));
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(object), typeof(StatusLabel));
    public object Label
    {
        get
        {
            return GetValue(LabelProperty);
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

    public static readonly DependencyProperty GapProperty = DependencyProperty.Register(nameof(Gap), typeof(double), typeof(StatusLabel), new PropertyMetadata(4d));
    public double Gap
    {
        get
        {
            return (double)GetValue(GapProperty);
        }
        set
        {
            SetValue(GapProperty, value);
        }
    }

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(StatusLabel), new PropertyMetadata(8d));
    public double IconSize
    {
        get
        {
            return (double)GetValue(IconSizeProperty);
        }
        set
        {
            SetValue(IconSizeProperty, value);
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
