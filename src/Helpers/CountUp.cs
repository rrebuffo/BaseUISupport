using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace BaseUISupport.Helpers;

public class CountUp : INotifyPropertyChanged
{
    private readonly DispatcherTimer Timer;
    public TimeSpan Elapsed;
    private DateTime LastStart;
    private readonly string TimeFormat;
    private readonly bool ShowMinutes = false;
    private readonly TimePrecision Precision = TimePrecision.Deciseconds;
    public string Name = "";

    public TimeSpan CurrentTime;


    private bool _isrunning = false;
    public bool IsRunning
    {
        get
        {
            return _isrunning;
        }
        set
        {
            _isrunning = value;
            OnPropertyChanged(nameof(IsRunning));
        }
    }

    public string Set
    {
        get
        {
            return Text;
        }
        set
        {
            try
            {
                if (value.IndexOf(':') < 0)
                {
                    if (value.IndexOf('.') < 0)
                    {
                        SetTime(TimeSpan.Parse("0.0:0:" + value + ".0"));
                    }
                    else
                    {
                        if (value.IndexOf('.') == 0)
                        {
                            SetTime(TimeSpan.Parse("0.0:0:0" + value));
                        }
                        else
                        {
                            SetTime(TimeSpan.Parse("0.0:0:" + value));
                        }
                    }
                }
                else
                {
                    if (value.Split(':').Length == 2)
                    {
                        SetTime(TimeSpan.Parse("0.0:" + value));
                    }
                    else if (value.Split(':').Length == 3)
                    {
                        SetTime(TimeSpan.Parse("0." + value));
                    }
                }
                OnPropertyChanged(nameof(Set));
            }
            catch (Exception) { }
        }
    }

    public string Text
    {
        get
        {
            return string.Format(TimeFormat, Math.Floor(CurrentTime.TotalMinutes), CurrentTime.Seconds, CurrentTime.Milliseconds / 100);
        }

    }

    private void SetTime(TimeSpan t)
    {
        Elapsed = t;
        if (IsRunning) LastStart = DateTime.Now;
        OnTimeChanged();
    }

    public CountUp(bool showMinutes = false, bool showFraction = false, TimePrecision precision = TimePrecision.Deciseconds)
    {
        Precision = precision;
        ShowMinutes = showMinutes;

        var interval = Precision switch
        {
            TimePrecision.Seconds => 1000,
            TimePrecision.Deciseconds => 100,
            TimePrecision.Centiseconds => 10,
            TimePrecision.Milliseconds => 1,
            _ => 100
        };
        Timer = new(DispatcherPriority.Send)
        {
            Interval = TimeSpan.FromMilliseconds(interval)
        };
        Timer.Tick += CountTick;
        Elapsed = TimeSpan.Zero;
        CurrentTime = TimeSpan.Zero;
        var fraction = showFraction ? ".{2:0}" : "";
        TimeFormat = ShowMinutes ? "{0:00}:{1:00}" + fraction : "{1:00}" + fraction;
        OnPropertyChanged(nameof(Text));
    }

    public void Start()
    {
        LastStart = DateTime.Now;
        IsRunning = true;
        Timer.Start();
    }

    public void Stop()
    {
        if (IsRunning)
        {
            Elapsed += (DateTime.Now - LastStart);
            IsRunning = false;
            Timer.Stop();
        }
    }

    public void Reset()
    {
        Elapsed = TimeSpan.Zero;
        CurrentTime = Elapsed;
        OnTimeChanged();
        OnPropertyChanged(nameof(Text));
    }

    private void Add(TimeSpan ammount)
    {
        Elapsed += ammount;
        if (!IsRunning) CurrentTime = Elapsed;
        OnTimeChanged();
    }
    private void Substract(TimeSpan ammount)
    {
        Elapsed = (Elapsed - ammount >= TimeSpan.Zero) ? Elapsed - ammount : TimeSpan.Zero;
        if (!IsRunning) CurrentTime = Elapsed;
        OnTimeChanged();
    }

    public void AddSecond()
    {
        Add(TimeSpan.FromSeconds(1));
    }

    public void SubSecond()
    {
        Substract(TimeSpan.FromSeconds(1));
    }

    public void AddTenth()
    {
        Add(TimeSpan.FromMilliseconds(100));
    }

    public void SubTenth()
    {
        Substract(TimeSpan.FromMilliseconds(100));
    }

    private void CountTick(object? sender, EventArgs e)
    {
        if (!IsRunning) return;
        TimeSpan elapsed = IsRunning ? Elapsed + (DateTime.Now - LastStart) : Elapsed;
        CurrentTime = elapsed;
        OnTimeChanged();
    }

    public string GetString(string f)
    {
        TimeSpan elapsed = IsRunning ? Elapsed + (DateTime.Now - LastStart) : Elapsed;
        return elapsed.ToString(f);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event EventHandler? TimeChanged;
    private protected virtual void OnTimeChanged()
    {
        OnPropertyChanged(nameof(Text));
        TimeChanged?.Invoke(this, new EventArgs());
    }
}
