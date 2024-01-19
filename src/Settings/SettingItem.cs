using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace BaseUISupport.Settings;

public class SettingItem : INotifyPropertyChanged
{
    public SettingItem? Parent { get; set; }

    [JsonIgnore]
    public string? Type { get; set; }

    [JsonPropertyName("Type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? JsonType 
    {
        get => Type == "s" ? null : Type;
        set => Type = value ?? "s";
    }

    [JsonIgnore]
    public Dictionary<string, SettingItem> Settings { get; set; } = [];

    [JsonPropertyName("Settings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, SettingItem>? JsonSettings
    {
        get => Settings?.Count > 0 ? Settings : null;
        set => Settings = value ?? [];
    }

    [JsonIgnore]
    public ObservableCollection<SettingValue> Values { get; set; } = [];

    [JsonPropertyName("Values")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ObservableCollection<SettingValue>? JsonValues
    {
        get => Values?.Count > 0 ? Values : null;
        set => Values = value ?? [];
    }

    public SettingItem(SettingItem? parent)
    {
        //Key = key;
        Parent = parent;
    }

    public SettingItem()
    {
        //Key = "";
    }

    [JsonIgnore]
    public bool HasChildren
    {
        get
        {
            return Settings.Any();
        }
    }

    [JsonIgnore]
    public string Path
    {
        get
        {
            StringBuilder path = new();
            SettingItem? s = this;
            while (s?.Parent is not null)
            {
                if (!string.IsNullOrEmpty(s.Parent.Key))
                {
                    path.Insert(0, '/');
                    path.Insert(0, s.Parent.Key);
                }
                s = s.Parent;
            }
            if (s is not null && s.Parent is null) path.Insert(0, '/');
            return path.ToString();
        }
    }

    [JsonIgnore]
    public string Key
    {
        get
        {
            if (Parent is null) return "";
            else
            {
                return Parent.Settings.First(s => s.Value == this).Key;
            }
        }
    }

    [JsonIgnore]
    public string FullPath
    {
        get
        {
            StringBuilder path = new();
            path.Append(Path);
            path.Append(Key);
            if (HasChildren || Values.Count == 0) path.Append('/');
            return path.ToString();
        }
    }

    [JsonIgnore]
    public string? Value
    {
        get
        {
            return Values.DefaultIfEmpty(null).First()?.Value;
        }
        set
        {
            if (Values.Count == 1)
            {
                if (Values.First()?.Value != value)
                {
                    Values[0].Value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
            else
            {
                Values.Clear();
                Values.Add(new() { Value = value });
            }
        }
    }

    public object? GetValue()
    {
        return SettingStore.GetValue(this);
    }

    public bool AddValue(string? value)
    {
        try
        {
            Values.Add(new() { Value = value });
            OnPropertyChanged(nameof(Value));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public SettingItem? AddSetting(string key)
    {
        if (GetSetting(key) is SettingItem s) return s;
        try
        {
            var newSetting = new SettingItem(this);
            Settings.Add(key, newSetting);
            return newSetting;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public SettingItem? GetSetting(string key)
    {
        return Settings.Any() && Settings.ContainsKey(key) ? Settings[key] : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        SettingStore.Save();
    }
}