using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace BaseUISupport.Helpers;

public class LocalizationHelper : INotifyPropertyChanged
{
    private readonly ResourceManager? resources;
    private static LocalizationHelper? instance;
    
    public static void Init(ResourceManager manager)
    {
        instance = new(manager);
    }

    public LocalizationHelper(ResourceManager manager)
    {
        resources = manager;
    }

    public static LocalizationHelper? Instance
    {
        get { return instance; }
    }

    public string this[string key]
    {
        get { return resources?.GetString(key, _currentCulture) ?? ""; }
    }

    private CultureInfo? _currentCulture = null;
    public CultureInfo CurrentCulture
    {
        get
        {
            return _currentCulture ?? CultureInfo.InvariantCulture;
        }
        set
        {
            if (_currentCulture != value)
            {
                _currentCulture = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged() { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty)); }
}

public class LocalizationExtension : Binding
{
    public LocalizationExtension(string name) : base("[" + name + "]")
    {
        Mode = BindingMode.OneWay;
        Source = LocalizationHelper.Instance;
    }
}