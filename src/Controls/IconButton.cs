using System.Windows;
using System.Windows.Controls;

namespace BaseUISupport.Controls;

public class IconButton : Button
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Style), typeof(IconButton));
    public Style Icon
    {
        get
        {
            return (Style)GetValue(IconProperty);
        }
        set
        {
            SetValue(IconProperty, value);
        }
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(IconButton));
    public Orientation Orientation
    {
        get
        {
            return (Orientation)GetValue(OrientationProperty);
        }
        set
        {
            SetValue(OrientationProperty, value);
        }
    }

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(IconButton), new(0d));
    public double Size
    {
        get
        {
            return (double)GetValue(SizeProperty);
        }
        set
        {
            SetValue(SizeProperty, value);
        }
    }

    public static readonly DependencyProperty GapProperty = DependencyProperty.Register("Gap", typeof(double), typeof(IconButton), new(0d));
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

}
