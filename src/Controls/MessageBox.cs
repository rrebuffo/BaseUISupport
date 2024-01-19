using BaseUISupport.Dialogs;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace BaseUISupport.Controls;

public static class MessageBox
{
    public static MessageBoxResult Show(string message)
    {
        MessageBoxResult result = ShowMessageBox(message, "", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        return result;
    }

    public static MessageBoxResult Show(string message, string caption)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        return result;
    }

    public static MessageBoxResult Show(string message, string caption, MessageBoxButton button)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        return result;
    }

    public static MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
        return result;
    }

    public static MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, icon, defaultResult, MessageBoxOptions.None);
        return result;
    }

    public static MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, icon, defaultResult, options);
        return result;
    }
    public static MessageBoxResult Show(Window owner, string message)
    {
        MessageBoxResult result = ShowMessageBox(message, "", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None, owner);
        return result;
    }

    public static MessageBoxResult Show(Window owner, string message, string caption)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None, owner);
        return result;
    }

    public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton button)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None, owner);
        return result;
    }

    public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None, owner);
        return result;
    }

    public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, icon, defaultResult, MessageBoxOptions.None, owner);
        return result;
    }

    public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
    {
        MessageBoxResult result = ShowMessageBox(message, caption, button, icon, defaultResult, options, owner);
        return result;
    }

    private static MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None, Window? owner = null)
    {
        MessageBoxResult result = defaultResult;

        Button OK_Button = new() { IsDefault = defaultResult == MessageBoxResult.OK, Content = Application.Current.TryFindResource("LocaleString_MessageBox_OK_Button") ?? "OK" };
        Button Cancel_Button = new() { IsCancel = true, IsDefault = defaultResult == MessageBoxResult.Cancel, Content = Application.Current.TryFindResource("LocaleString_MessageBox_Cancel_Button") ?? "Cancel" };
        Button Yes_Button = new() { IsDefault = defaultResult == MessageBoxResult.Yes, Content = Application.Current.TryFindResource("LocaleString_MessageBox_Yes_Button") ?? "Yes" };
        Button No_Button = new() { IsDefault = defaultResult == MessageBoxResult.No, Content = Application.Current.TryFindResource("LocaleString_MessageBox_No_Button") ?? "No" };

        Button[]? buttons = button switch
        {
            MessageBoxButton.OK => new Button[] { OK_Button },
            MessageBoxButton.OKCancel => new Button[] { OK_Button, Cancel_Button },
            MessageBoxButton.YesNoCancel => new Button[] { Yes_Button, No_Button, Cancel_Button },
            MessageBoxButton.YesNo => new Button[] { Yes_Button, No_Button },
            _ => null
        };

        MessageDialog dialog = new()
        {
            WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            Button = button,
            Image = icon,
            Caption = caption,
            Message = message,
            Options = options
        };
        dialog.Owner = Application.Current.MainWindow != dialog ? Application.Current.MainWindow : null;
        if (dialog.Owner is null) dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        OK_Button.Click += (object? sender, RoutedEventArgs e) => { result = MessageBoxResult.OK; dialog.Close(); };
        Cancel_Button.Click += (object? sender, RoutedEventArgs e) => { result = MessageBoxResult.Cancel; dialog.Close(); };
        Yes_Button.Click += (object? sender, RoutedEventArgs e) => { result = MessageBoxResult.Yes; dialog.Close(); };
        No_Button.Click += (object? sender, RoutedEventArgs e) => { result = MessageBoxResult.No; dialog.Close(); };

        dialog.ButtonRow.ItemsSource = buttons;
        dialog.ShowDialog();
        return result;
    }

    public static MessageDialog Edit(string caption, EditBoxField[] fields, string? editLabel = null, string? cancelLabel = null)
    {
        Button Edit_Button = new() { IsDefault = true, Content = editLabel ?? Application.Current.TryFindResource("LocaleString_EditBox_Edit_Button") ?? "Edit" };
        Button Cancel_Button = new() { IsCancel = true, Content = cancelLabel ?? Application.Current.TryFindResource("LocaleString_EditBox_Cancel_Button") ?? "Cancel" };
        Button[] buttons = new Button[] { Edit_Button, Cancel_Button };

        MessageDialog dialog = new()
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Caption = caption,
            Fields = fields
        };

        Edit_Button.Click += (object? sender, RoutedEventArgs e) => { EditBoxField.EditFields(fields); dialog.Result = MessageBoxResult.OK; dialog.Close(); };
        Cancel_Button.Click += (object? sender, RoutedEventArgs e) => { dialog.Result = MessageBoxResult.Cancel; dialog.Close(); };
         
        dialog.ButtonRow.ItemsSource = buttons;
        dialog.ShowDialog();
        return dialog;
    }

}

public class EditBoxField : INotifyPropertyChanged
{

    public static void EditFields(EditBoxField[] fields)
    {
        if (fields is null) return;
        foreach(EditBoxField field in fields)
        {
            if (field.TargetObject is not null && !string.IsNullOrEmpty(field.FieldProperty) && field.TargetObject.GetType().GetProperty(field.FieldProperty) is PropertyInfo property)
            {
                property.SetValue(field.TargetObject, field.FieldValue);
            }
        }
    }

    private string _fieldName = "";
    public string FieldName
    {
        get
        {
            return _fieldName;
        }
        set
        {
            if (_fieldName != value)
            {
                _fieldName = value;
                OnPropertyChanged(nameof(FieldName));
            }
        }
    }

    private string _fieldProperty = "";
    public string FieldProperty
    {
        get
        {
            return _fieldProperty;
        }
        set
        {
            if (_fieldProperty != value)
            {
                _fieldProperty = value;
                _fieldValue = TargetValue is string s ? s : "";
                OnPropertyChanged(nameof(FieldProperty));
            }
        }
    }

    private object? _targetObject = null;
    public object? TargetObject
    {
        get
        {
            return _targetObject;
        }
        set
        {
            if (_targetObject != value)
            {
                _targetObject = value;
                _fieldValue = TargetValue is string s ? s : "";
                OnPropertyChanged(nameof(TargetObject));
            }
        }
    }

    private string _fieldValue = "";
    public string FieldValue
    {
        get
        {
            return _fieldValue;
        }
        set
        {
            if (_fieldValue != value)
            {
                _fieldValue = value;
                OnPropertyChanged(nameof(FieldValue));
            }
        }
    }

    public object? TargetValue
    {
        get
        {
            if(TargetObject is not null && !string.IsNullOrEmpty(FieldProperty) && TargetObject.GetType().GetProperty(FieldProperty) is object value)
            {

                return value;
            }
            return null;
        }
    }

    public EditBoxField(string fieldName, object targetObject, string fieldProperty)
    {
        FieldName = fieldName;
        TargetObject = targetObject;
        FieldProperty = fieldProperty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

}
