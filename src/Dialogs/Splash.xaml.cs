using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BaseUISupport.Dialogs;

/// <summary>
/// Lógica de interacción para Splash.xaml
/// </summary>
public partial class Splash : Window, INotifyPropertyChanged
{
    private readonly DispatcherTimer CloseTimer = new();
    public bool ManuallyClosed { get; private set; } = false;
    
    public Splash(BitmapImage image, double width, double height)
    {
        SplashWidth = width;
        SplashHeight = height;
        InitializeComponent();
        SplashContainer.Source = image;
        MouseDown += Splash_MouseDown;
        Hide();
    }

    public void DelayedClose(TimeSpan? time = null)
    {
        CloseTimer.Interval = time ?? TimeSpan.FromSeconds(.2);
        CloseTimer.Tick += CloseTimer_Tick;
        CloseTimer.Start();
    }

    private void CloseTimer_Tick(object? sender, EventArgs e)
    {
        CloseTimer.Stop();
        CloseTimer.Tick -= CloseTimer_Tick;
        Close();
    }

    public void ShowCloseButton()
    {
        ShowClose = true;
    }

    public void HideCloseButton()
    {
        ShowClose = false;
    }

    private string currentStatus = "";
    public string CurrentStatus
    {
        get
        {
            return currentStatus;
        }
        set
        {
            if (currentStatus != value)
            {
                currentStatus = value;
                OnPropertyChanged(nameof(CurrentStatus));
            }
        }
    }

    private string splashLegend = "";
    public string SplashLegend
    {
        get
        {
            return splashLegend;
        }
        set
        {
            if (splashLegend != value)
            {
                splashLegend = value;
                OnPropertyChanged(nameof(SplashLegend));
            }
        }
    }

    private double width = 500;
    public double SplashWidth
    {
        get
        {
            return width;
        }
        set
        {
            if (width != value)
            {
                width = value;
                OnPropertyChanged(nameof(SplashWidth));
            }
        }
    }

    private double height = 350;
    public double SplashHeight
    {
        get
        {
            return height;
        }
        set
        {
            if (height != value)
            {
                height = value;
                OnPropertyChanged(nameof(SplashHeight));
            }
        }
    }

    private bool showClose = false;
    public bool ShowClose
    {
        get
        {
            return showClose;
        }
        set
        {
            if (showClose != value)
            {
                showClose = value;
                OnPropertyChanged(nameof(ShowClose));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void CloseApp_Click(object? sender, RoutedEventArgs e)
    {
        ManuallyClosed = true;
        Close();
    }

    private void Splash_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void SplashContainer_Loaded(object? sender, RoutedEventArgs e)
    {
        Show();
    }
}
