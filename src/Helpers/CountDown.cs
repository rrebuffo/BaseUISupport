using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace BaseUISupport.Helpers;

public class CountDown : INotifyPropertyChanged
{
    private readonly DispatcherTimer Timer;
    public TimeSpan CurrentTimeSpan;
    private DateTime CurrentTarget;
    private readonly string TimeFormat;
    private readonly bool ShowMinutes = false;
    private readonly bool ShowFraction = false;
    private readonly bool AllowOverflow = false;
    private readonly TimePrecision Precision = TimePrecision.Deciseconds;
    public string Name = "";

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

    private TimeSpan countDownTime;
    public TimeSpan CountDownTime
    {
        get
        {
            return countDownTime;
        }
        set
        {
            if (countDownTime != value)
            {
                countDownTime = value;
                OnPropertyChanged(nameof(CountDownTime));
            }
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
            bool isNegative = CurrentTimeSpan < TimeSpan.Zero;
            TimeSpan current = CurrentTimeSpan;
            if (isNegative)
            {
                TimeSpan time = ShowFraction ? current : TimeSpan.FromSeconds(Math.Ceiling(current.TotalSeconds));
                return "-" + string.Format(TimeFormat, Math.Abs(Math.Ceiling(time.TotalMinutes)), (!ShowMinutes && time.TotalSeconds <= -60) ? Math.Abs(Math.Ceiling(time.TotalSeconds)) : Math.Abs(time.Seconds), Math.Abs(time.Milliseconds) / 100);
            }
            else
            {
                TimeSpan time = ShowFraction ? current : TimeSpan.FromSeconds(Math.Ceiling(current.TotalSeconds));
                return string.Format(TimeFormat, Math.Floor(time.TotalMinutes), (!ShowMinutes && time.TotalSeconds <= 60) ? Math.Floor(time.TotalSeconds) : time.Seconds, time.Milliseconds / 100);
            }

        }

    }

    public CountDown(TimeSpan t, bool showMinutes = false, bool showFraction = false, TimePrecision precision = TimePrecision.Deciseconds, bool allowOverflow = false)
    {
        Precision = precision;
        ShowFraction = showFraction;
        ShowMinutes = showMinutes;
        AllowOverflow = allowOverflow;

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
        Timer.Tick += CountDownTick;
        CountDownTime = t;
        CurrentTimeSpan = t;
        var fraction = showFraction ? ".{2:0}" : "";
        TimeFormat = showMinutes ? "{0:00}:{1:00}" + fraction : "{1:00}" + fraction;
        OnPropertyChanged(nameof(Text));
    }

    public void Start()
    {
        if (CurrentTimeSpan == TimeSpan.Zero) return;
        IsRunning = true;
        CurrentTarget = DateTime.Now + CurrentTimeSpan;
        Timer.Start();
    }

    public void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            CurrentTimeSpan = CurrentTarget - DateTime.Now;
            Timer.Stop();
        }
    }

    public void Reset()
    {
        reachedZero = false;
        CurrentTimeSpan = CountDownTime;
        CurrentTarget = DateTime.Now + CurrentTimeSpan;
        OnTimeChanged();
        OnPropertyChanged(nameof(Text));
    }

    public void Reset(TimeSpan time)
    {
        reachedZero = false;
        CurrentTimeSpan = time;
        CurrentTarget = DateTime.Now + CurrentTimeSpan;
        OnTimeChanged();
    }

    private void Add(TimeSpan ammount)
    {
        if (IsRunning) CurrentTarget += ammount;
        CurrentTimeSpan += ammount;
        OnTimeChanged();
    }
    private void Substract(TimeSpan ammount)
    {
        if (IsRunning) CurrentTarget -= ammount;
        CurrentTimeSpan = (CurrentTimeSpan - ammount >= TimeSpan.Zero && !AllowOverflow) ? CurrentTimeSpan - ammount : TimeSpan.Zero;
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

    bool reachedZero = false;

    private void CountDownTick(object? sender, EventArgs e)
    {
        if (!IsRunning) return;
        TimeSpan remaining = CurrentTarget - DateTime.Now;
        
        if (remaining.TotalMilliseconds < 1)
        {
            if (!AllowOverflow)
            {
                remaining = TimeSpan.Zero;
                IsRunning = false;
                OnTimeEnded();
            }
            else
            {
                if(!reachedZero)
                {
                    reachedZero = true;
                    OnReachedZero();
                }
            }
        }
        
        CurrentTimeSpan = remaining;
        OnTimeChanged();
    }

    public string GetString(string f)
    {
        TimeSpan remaining = CurrentTimeSpan;
        if (IsRunning) remaining = CurrentTarget - DateTime.Now;
        if (!IsRunning) remaining = CurrentTimeSpan;
        return remaining.ToString(f);
    }

    public void SetTime(TimeSpan t)
    {
        CurrentTimeSpan = t;
        countDownTime = t;
        CurrentTarget = DateTime.Now + t;
        OnTimeChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event EventHandler? TimeChanged;
    protected virtual void OnTimeChanged()
    {
        OnPropertyChanged(nameof(Text));
        TimeChanged?.Invoke(this, new EventArgs());
    }

    public event EventHandler? TimeEnded;
    protected virtual void OnTimeEnded()
    {
        OnPropertyChanged(nameof(Text));
        TimeEnded?.Invoke(this, new EventArgs());
    }

    public event EventHandler? ReachedZero;
    protected virtual void OnReachedZero()
    {
        OnPropertyChanged(nameof(Text));
        ReachedZero?.Invoke(this, new EventArgs());
    }
}

public enum TimePrecision
{
    Seconds,
    Deciseconds,
    Centiseconds,
    Milliseconds
}
