using BaseUISupport.Helpers;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace BaseUISupport.Controls;

[TemplatePart(Name = "PART_CloseTabButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_ItemBorder", Type = typeof(Border))]
[TemplatePart(Name = "ContentSite", Type = typeof(ContentPresenter))]
[TemplatePart(Name = "PART_RenameTabTextBox", Type = typeof(TextBox))]

public class TabbedPanelItem : TabItem, INotifyPropertyChanged
{
    private Button? closeButton;
    private Border? itemBorder;
    private TextBox? renameTextBox;
    private ContentPresenter? contentPresenter;
    private bool isRenaming = false;

    public TabbedPanelItem()
    {

    }

    public static readonly RoutedEvent TabClosedEvent = EventManager.RegisterRoutedEvent("TabClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TabbedPanelItem));
    public static void AddTabClosedHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie)
        {
            uie.AddHandler(TabClosedEvent, handler);
        }
    }

    public static void RemoveTabClosedHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie)
        {
            uie.RemoveHandler(TabClosedEvent, handler);
        }
    }


    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if(Template.FindName("PART_CloseTabButton", this) is Button close)
        {
            closeButton = close;
            closeButton.Click += CloseButton_Click;
        }
        if (Template.FindName("PART_ItemBorder", this) is Border border) itemBorder = border;
        if (Template.FindName("PART_RenameTabTextBox", this) is TextBox rename) renameTextBox = rename;
        if (Template.FindName("ContentSite", this) is ContentPresenter content) contentPresenter = content;
        MouseDoubleClick += RenameButton_MouseDoubleClick;
    }

    private void RenameButton_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource != itemBorder) return;
        if (isRenaming) return;
        e.Handled = true;
        ReleaseMouseCapture();
        if (this.FindParent<TabbedPanel>() is not TabbedPanel parent) return;
        if (parent.CanRenameTabs) StartRenaming();
    }

    private void StartRenaming()
    {
        if (contentPresenter is null || renameTextBox is null) return;
        isRenaming = true;
        contentPresenter.Visibility = Visibility.Collapsed;
        renameTextBox.Visibility = Visibility.Visible;
        renameTextBox.GotFocus += RenameTextBox_GotFocus;
        renameTextBox.Focus();
    }

    private void RenameTextBox_GotFocus(object? sender, RoutedEventArgs e)
    {
        if (renameTextBox is null) return;
        renameTextBox.GotFocus -= RenameTextBox_GotFocus;
        renameTextBox.ReleaseMouseCapture();
        renameTextBox.SelectAll();
        renameTextBox.KeyDown += RenameTextBox_KeyDown;
        renameTextBox.LostFocus += RenameTextBox_LostFocus;
    }

    private void EndRenaming(bool save = false)
    {
        if (contentPresenter is null || renameTextBox is null) return;
        isRenaming = false;
        contentPresenter.Visibility = Visibility.Visible;
        renameTextBox.Visibility = Visibility.Collapsed;
        renameTextBox.KeyDown -= RenameTextBox_KeyDown;
        renameTextBox.LostFocus -= RenameTextBox_LostFocus;
        BindingExpression be = renameTextBox.GetBindingExpression(TextBox.TextProperty);
        if (save) be.UpdateSource();
        else be.UpdateTarget();
    }

    private void RenameTextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        EndRenaming();
    }

    private void RenameTextBox_KeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape) EndRenaming();
        if (e.Key == Key.Enter) EndRenaming(true);
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(TabClosedEvent));
    }

    public static readonly DependencyProperty DockToEndProperty = DependencyProperty.Register("DockToEnd", typeof(Boolean), typeof(TabbedPanelItem), new PropertyMetadata(false));
    public Boolean DockToEnd
    {
        get
        {
            return (Boolean)GetValue(DockToEndProperty);
        }
        set
        {
            SetValue(DockToEndProperty, value);
        }
    }

    public static readonly DependencyProperty DetailBlockProperty = DependencyProperty.Register("DetailBlock", typeof(object), typeof(TabbedPanelItem));
    public object DetailBlock
    {
        get
        {
            return GetValue(DetailBlockProperty);
        }
        set
        {
            SetValue(DetailBlockProperty, value);
        }
    }

    public static readonly DependencyProperty CommandsBlockProperty = DependencyProperty.Register("CommandsBlock", typeof(object), typeof(TabbedPanelItem));
    public object CommandsBlock
    {
        get
        {
            return GetValue(CommandsBlockProperty);
        }
        set
        {
            SetValue(CommandsBlockProperty, value);
        }
    }

    public static readonly DependencyProperty StatusBlockProperty = DependencyProperty.Register("StatusBlock", typeof(object), typeof(TabbedPanelItem));
    public object StatusBlock
    {
        get
        {
            return GetValue(StatusBlockProperty);
        }
        set
        {
            SetValue(StatusBlockProperty, value);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
