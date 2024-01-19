using System.Windows;
using System.Windows.Controls.Primitives;

namespace BaseUISupport.Controls;

public class LockableToggleButton : ToggleButton
{
    protected override void OnToggle()
    {
        if (!LockToggle)
        {
            base.OnToggle();
        }
    }

    public static readonly DependencyProperty LockToggleProperty = DependencyProperty.Register("LockToggle", typeof(bool), typeof(LockableToggleButton), new UIPropertyMetadata(false));
    public bool LockToggle
    {
        get => (bool)GetValue(LockToggleProperty);
        set => SetValue(LockToggleProperty, value);
    }

}
