using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BaseUISupport.Controls;


public class TabbedPanel : TabControl, INotifyPropertyChanged
{
    public TabbedPanel()
    {

    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TabbedPanelItem();
    }

    public static readonly DependencyProperty CommandIconsProperty = DependencyProperty.Register("CommandIcons", typeof(object[]), typeof(TabbedPanel));

    public object CommandIcons
    {
        get
        {
            return GetValue(CommandIconsProperty);
        }
        set
        {
            SetValue(CommandIconsProperty, value);
        }
    }

    public static readonly DependencyProperty CanCloseTabsProperty = DependencyProperty.Register("CanCloseTabs", typeof(bool), typeof(TabbedPanel));

    public bool CanCloseTabs
    {
        get
        {
            return (bool)GetValue(CanCloseTabsProperty);
        }
        set
        {
            SetValue(CanCloseTabsProperty, value);
        }
    }

    public static readonly DependencyProperty CanRenameTabsProperty = DependencyProperty.Register("CanRenameTabs", typeof(bool), typeof(TabbedPanelItem));
    public bool CanRenameTabs
    {
        get
        {
            return (bool)GetValue(CanRenameTabsProperty);
        }
        set
        {
            SetValue(CanRenameTabsProperty, value);
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
