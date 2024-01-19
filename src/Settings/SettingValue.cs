using System.ComponentModel;

namespace BaseUISupport.Settings;

public class SettingValue : INotifyPropertyChanged
{
    public SettingValue()
    {

    }

    public string? Value { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
}
