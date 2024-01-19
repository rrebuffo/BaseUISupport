using System.Diagnostics;

namespace BaseUISupport.Settings;


public static class SettingsExtensions
{
    public static T UpdateSetting<T>(this T source, string path)
    {
        Debug.WriteLine(typeof(T) + " : " + SettingStore.Get(path)?.Value?.GetType());
        T? newValue = source;
        /*try
        {
            newValue = (T?)SettingStore.Get(path)?.Value ?? source;
        }
        catch { Debug.WriteLine("not casteable"); }*/
        return newValue;
    }
}