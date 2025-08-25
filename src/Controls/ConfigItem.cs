using BaseUISupport.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BaseUISupport.Controls;

[ContentProperty(nameof(InnerContent))]
public class ConfigItem : Control
{

    static ConfigItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ConfigItem), new FrameworkPropertyMetadata(typeof(ConfigItem)));
    }

    public static readonly DependencyProperty CollapsedProperty = DependencyProperty.Register(nameof(Collapsed), typeof(bool), typeof(ConfigItem), new(false));
    public bool Collapsed
    {
        get
        {
            return (bool)GetValue(CollapsedProperty);
        }
        set
        {
            SetValue(CollapsedProperty, value);
        }
    }

    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(ConfigurationItemType), typeof(ConfigItem), new(ConfigurationItemType.Custom));
    public ConfigurationItemType Type
    {
        get
        {
            return (ConfigurationItemType)GetValue(TypeProperty);
        }
        set
        {
            SetValue(TypeProperty, value);
        }
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(ConfigItem));
    public string Label
    {
        get
        {
            return (string)GetValue(LabelProperty);
        }
        set
        {
            SetValue(LabelProperty, value);
        }
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(ConfigItem));
    public string Description
    {
        get
        {
            return (string)GetValue(DescriptionProperty);
        }
        set
        {
            SetValue(DescriptionProperty, value);
        }
    }

    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(nameof(Actions), typeof(object), typeof(ConfigItem));
    public object Actions
    {
        get
        {
            return GetValue(ActionsProperty);
        }
        set
        {
            SetValue(ActionsProperty, value);
        }
    }

    public static readonly DependencyProperty BindingProperty = DependencyProperty.Register(nameof(Binding), typeof(object), typeof(ConfigItem), new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true});
    public object Binding
    {
        get
        {
            return GetValue(BindingProperty);
        }
        set
        {
            SetValue(BindingProperty, value);
        }
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(ConfigItem));
    public object ItemsSource
    {
        get
        {
            return GetValue(ItemsSourceProperty);
        }
        set
        {
            SetValue(ItemsSourceProperty, value);
        }
    }

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(FrameworkTemplate), typeof(ConfigItem));
    public FrameworkTemplate ItemTemplate
    {
        get
        {
            return (FrameworkTemplate)GetValue(ItemTemplateProperty);
        }
        set
        {
            SetValue(ItemTemplateProperty, value);
        }
    }

    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(nameof(DisplayMemberPath), typeof(object), typeof(ConfigItem));
    public object? DisplayMemberPath
    {
        get
        {
            return GetValue(DisplayMemberPathProperty);
        }
        set
        {
            SetValue(DisplayMemberPathProperty, value);
        }
    }

    public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register(nameof(SelectedValuePath), typeof(object), typeof(ConfigItem), new(""));
    public object? SelectedValuePath
    {
        get
        {
            return GetValue(SelectedValuePathProperty);
        }
        set
        {
            SetValue(SelectedValuePathProperty, value);
        }
    }

    public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register(nameof(InnerContent), typeof(object), typeof(ConfigItem));
    public object InnerContent
    {
        get
        {
            return GetValue(InnerContentProperty);
        }
        set
        {
            SetValue(InnerContentProperty, value);
        }
    }

    public static readonly DependencyProperty ShowDescriptionBelowProperty = DependencyProperty.Register("ShowDescriptionBelow", typeof(bool), typeof(ConfigItem), new UIPropertyMetadata(false));
    public bool ShowDescriptionBelow
    {
        get
        {
            return (bool)GetValue(ShowDescriptionBelowProperty);
        }
        set
        {
            SetValue(ShowDescriptionBelowProperty, value);
        }
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ConfigItem), new UIPropertyMetadata(false));
    public bool IsReadOnly
    {
        get
        {
            return (bool)GetValue(IsReadOnlyProperty);
        }
        set
        {
            SetValue(IsReadOnlyProperty, value);
        }
    }

    public static readonly DependencyProperty LiveUpdateProperty = DependencyProperty.Register("LiveUpdate", typeof(bool), typeof(ConfigItem), new UIPropertyMetadata(false));
    public bool LiveUpdate
    {
        get
        {
            return (bool)GetValue(LiveUpdateProperty);
        }
        set
        {
            SetValue(LiveUpdateProperty, value);
        }
    }
}


public enum ConfigurationItemType
{
    Custom,
    Text,
    Integer,
    ComboBox,
    CheckBox
}

public class ConfigurationItemTypeTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? IntegerTemplate { get; set; }
    public DataTemplate? ComboBoxTemplate { get; set; }
    public DataTemplate? CheckBoxTemplate { get; set; }
    public DataTemplate? CustomTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (container.FindParent<ConfigItem>() is ConfigItem configItem)
        {
            return configItem.Type switch
            {
                ConfigurationItemType.Text => TextTemplate,
                ConfigurationItemType.Integer => IntegerTemplate,
                ConfigurationItemType.ComboBox => ComboBoxTemplate,
                ConfigurationItemType.CheckBox => CheckBoxTemplate,
                ConfigurationItemType.Custom or _ => CustomTemplate
            };
        }
        return CustomTemplate;
    }
}
