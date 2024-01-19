using System.Windows;
using System.Windows.Controls;

namespace BaseUISupport.Helpers;

public class TextBoxHelper
{
    public static bool GetAutoTooltip(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoTooltipProperty);
    }

    public static void SetAutoTooltip(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoTooltipProperty, value);
    }

    public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached("AutoTooltip", typeof(bool), typeof(TextBoxHelper), new(false, OnAutoTooltipPropertyChanged));

    private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock) return;

        if (e.NewValue.Equals(true))
        {
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            ComputeAutoTooltip(textBlock);
            textBlock.SizeChanged += TextBlock_SizeChanged;
        }
        else
        {
            textBlock.SizeChanged -= TextBlock_SizeChanged;
        }
    }

    private static void TextBlock_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if(sender is TextBlock textBlock)
        ComputeAutoTooltip(textBlock);
    }

    private static void ComputeAutoTooltip(TextBlock textBlock)
    {
        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var width = textBlock.DesiredSize.Width;

        if (textBlock.ActualWidth < width) ToolTipService.SetToolTip(textBlock, textBlock.Text);
        else ToolTipService.SetToolTip(textBlock, null);
    }
}