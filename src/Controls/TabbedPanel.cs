using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BaseUISupport.Controls;


public class TabbedPanel : TabControl, INotifyPropertyChanged
{
    public TabbedPanel()
    {
        SetValue(CommandButtonsProperty, new ObservableCollection<Button>());
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TabbedPanelItem();
    }

    public static readonly DependencyProperty CommandButtonsProperty = DependencyProperty.Register("CommandButtons", typeof(ObservableCollection<Button>), typeof(TabbedPanel), new UIPropertyMetadata());

    public ObservableCollection<Button> CommandButtons
    {
        get
        {
            return (ObservableCollection<Button>)GetValue(CommandButtonsProperty);
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


    public static readonly DependencyProperty ScrollTabsProperty = DependencyProperty.Register("ScrollTabs", typeof(bool), typeof(TabbedPanel), new UIPropertyMetadata(true));
    public bool ScrollTabs
    {
        get
        {
            return (bool)GetValue(ScrollTabsProperty);
        }
        set
        {
            SetValue(ScrollTabsProperty, value);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
