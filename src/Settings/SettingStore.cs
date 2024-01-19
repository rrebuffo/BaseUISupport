using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable SYSLIB0011

namespace BaseUISupport.Settings;

public class SettingStore
{
    public static SettingItem Root { get; private set; } = new(null);
    private static string filePath = string.Empty;
    private static readonly DispatcherTimer saveTimer = new(DispatcherPriority.Normal);

    public static bool WriteEnabled { get; set; } = false;

    public static void Init(string appId, bool global = false)
    {
        var folder = global ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), appId) : Directory.GetCurrentDirectory();
        var file = $"{appId}.settings";
        filePath =
            Directory.GetCurrentDirectory() is string d
            ? Path.Combine(d, file)
            : file;
        saveTimer.Tick += SaveTimer_Tick;
    }

    private static void SaveTimer_Tick(object? sender, EventArgs e)
    {
        saveTimer.Stop();
        SaveData();
    }

    public static SettingItem? Get(string path)
    {
        if (path == "" || path == "/") return Root;
        if (path.StartsWith('/')) path = path[1..];
        var steps = path.Split('/');
        var item = Root;

        foreach (var s in steps)
        {
            if (string.IsNullOrEmpty(s)) return item;
            if (item.GetSetting(s) is SettingItem match)
            {
                item = match;
            }
            else return null;
        }
        return item;
    }

    public static object? GetValue(string path)
    {
        if (Get(path) is not SettingItem match || match.Value is not string value) return null;
        return ConvertValue(match.Type ?? "s", value);
    }

    public static object? GetValue(SettingItem setting)
    {
        if (setting.Value is not string value) return null;
        return ConvertValue(setting.Type ?? "s", value);
    }

    public static List<object?> GetValues(string path)
    {
        List<object?> result = [];
        if (Get(path) is not SettingItem match || match.Values.Count == 0) return result;
        foreach (var value in match.Values)
        {
            if (value is null || value.Value is null) continue;
            result.Add(ConvertValue(match.Type ?? "s", value.Value));
        }
        return result;
    }

    private static object? ConvertValue(string type, string value)
    {
        switch (type)
        {
            case "s":
                return value;
            case "i":
                return int.Parse(value, CultureInfo.InvariantCulture);
            case "d":
                return double.Parse(value, CultureInfo.InvariantCulture);
            case "b":
                return bool.Parse(value);
            case "n":
                BinaryFormatter serialzer = new();
                MemoryStream ms = new(Convert.FromBase64String(value));
                return serialzer.Deserialize(ms);
            default:
                return null;
        }
    }

    public static SettingItem? Set(string path)
    {
        return SetSetting(path, "s", null);
    }

    public static SettingItem? Set(string path, string value)
    {
        return SetSetting(path, "s", value);
    }

    public static SettingItem? Set(string path, int value)
    {
        return SetSetting(path, "i", value.ToString(CultureInfo.InvariantCulture));
    }

    public static SettingItem? Set(string path, double value)
    {
        return SetSetting(path, "d", value.ToString(CultureInfo.InvariantCulture));
    }

    public static SettingItem? Set(string path, bool value)
    {
        return SetSetting(path, "b", value ? "true" : "false");
    }

    public static SettingItem? Set(string path, object value)
    {
        BinaryFormatter serialzer = new();
        MemoryStream ms = new();
        serialzer.Serialize(ms, value);
        ms.Position = 0;
        string serialized = Convert.ToBase64String(ms.ToArray());
        return SetSetting(path, "n", serialized);
    }

    public static SettingItem? GetOrSet(string path, object? defaultValue = null)
    {
        if (Get(path) is not SettingItem matched)
        {
            return defaultValue switch
            {
                bool b => Set(path, b),
                string s => Set(path, s),
                double d => Set(path, d),
                int i => Set(path, i),
                object o => Set(path, o),
                _ => Set(path)
            };
        }
        else return matched;

    }

    public static bool GetOrSetValue(string path, bool defaultValue)
    {
        if (Get(path) is not SettingItem matched)
        {
            Set(path, defaultValue);
            return defaultValue;
        }
        else return (bool)(matched.GetValue() ?? defaultValue);
    }

    public static int GetOrSetValue(string path, int defaultValue)
    {
        if (Get(path) is not SettingItem matched)
        {
            Set(path, defaultValue);
            return defaultValue;
        }
        else return (int)(matched.GetValue() ?? defaultValue);
    }

    public static double GetOrSetValue(string path, double defaultValue)
    {
        if (Get(path) is not SettingItem matched)
        {
            Set(path, defaultValue);
            return defaultValue;
        }
        else return (double)(matched.GetValue() ?? defaultValue);
    }

    public static string GetOrSetValue(string path, string defaultValue)
    {
        if (Get(path) is not SettingItem matched)
        {
            Set(path, defaultValue);
            return defaultValue;
        }
        else return (string)(matched.GetValue() ?? defaultValue);
    }

    public static object GetOrSetValue(string path, object defaultValue)
    {
        if (Get(path) is not SettingItem matched)
        {
            Set(path, defaultValue);
            return defaultValue;
        }
        else return matched.GetValue() ?? defaultValue;
    }

    private static SettingItem? SetSetting(string path, string type, string? value)
    {
        if (Get(path) is SettingItem match)
        {
            match.Type = type;
            match.Value = value;
            Save();
            return match;
        }
        else
        {
            var find = path;
            if (path == "" || path == "/") return Root;
            if (path.StartsWith('/')) find = path[1..];
            var steps = find.Split('/');
            SettingItem? item = Root;
            foreach (var s in steps)
            {
                if (string.IsNullOrEmpty(s) && item is not null) return item;
                SettingItem? parent = item?.GetSetting(s);
                if (parent is null)
                {
                    item = item?.AddSetting(s);
                    if (value is not null) item?.AddValue("");
                }
                else
                {
                    item = parent;
                }
            }
            if (item?.FullPath[1..] == find && value is not null)
            {
                item.Type = type;
                item.Value = value;
            }
            Save();
            return item;
        }
    }

    private static bool ClearSetting(string path)
    {
        if (Get(path) is SettingItem match)
        {
            try
            {
                if (match.Parent is SettingItem parent && parent.Settings.FirstOrDefault(m => m.Value == match) is KeyValuePair<string, SettingItem> item)
                {
                    parent.Settings.Remove(item.Key);
                    return true;
                }
            }
            catch { }
        }
        return false;
    }

    private static bool ClearSettings(string path)
    {
        if (Get(path) is SettingItem match)
        {
            try
            {
                if (match.Settings.Any())
                {
                    foreach(KeyValuePair<string,SettingItem> item in match.Settings)
                    {
                        match.Settings.Remove(item.Key);
                    }
                    return true;
                }
            }
            catch { }
        }
        return false;
    }

    internal static SettingItem? GetSetting(SettingItem parent, string key)
    {
        return parent.Settings.Any() && parent.Settings.ContainsKey(key) ? parent.Settings[key] : null;
    }

    internal static Dictionary<string,SettingItem>? GetSettings(string path)
    {
        var parent = Get(path);
        if (parent is not null)
        {
            return parent.Settings;
        }
        return null;
    }

    public static void Save(bool force = false)
    {
        if (force)
        {
            SaveData();
        }
        else
        {
            if (!WriteEnabled) return;
            saveTimer.Stop();
            saveTimer.Interval = TimeSpan.FromMilliseconds(500);
            saveTimer.Start();
        }
    }

    public static bool Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            string data = File.ReadAllText(filePath, Encoding.UTF8);
            if (JsonSerializer.Deserialize<SettingItem>(data, new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve, WriteIndented = true }) is SettingItem loadedSettings)
            {
                Root = loadedSettings;
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.InnerException);
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.Source);
            Debug.WriteLine(e.StackTrace);
            Debug.WriteLine(e.Data);
        }
        return false;
    }

    private static bool SaveData()
    {
        try
        {
            string data = JsonSerializer.Serialize(Root, typeof(SettingItem), new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    
                }
            );
            File.WriteAllText(filePath, data);
            return true;
        }
        catch { }
        return false;

    }

}
