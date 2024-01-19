using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Security;
using System.Windows.Media;
using System.Windows.Shell;
using System.Threading;
using System.Diagnostics;

namespace BaseUISupport.Controls;

public partial class AppWindow : Window, INotifyPropertyChanged, IDisposable
{

    public AppWindow()
    {

        bool style_is_set = false;

        while (!style_is_set)
        {
            try
            {
                Style = (Style)FindResource("AppWindowStyle");
                style_is_set = true;
            }
            catch (Exception)
            {
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/BaseUISupport/Styles/DarkColors.xaml", UriKind.Relative)));
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/BaseUISupport/Styles/ControlTemplates.xaml", UriKind.Relative)));
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/BaseUISupport/Styles/Icons.xaml", UriKind.Relative)));
            }

            Thread.Sleep(10);
        }

        MouseDown += AppWindow_MouseDown;

        var minBinding = new CommandBinding { Command = AppWindowMinimizeCommand };
        minBinding.Executed += MinBinding_Executed;
        minBinding.CanExecute += MinBinding_CanExecute;
        CommandBindings.Add(minBinding);

        var maxBinding = new CommandBinding { Command = AppWindowMaximizeCommand };
        maxBinding.Executed += MaxBinding_Executed;
        maxBinding.CanExecute += MaxBinding_CanExecute;
        CommandBindings.Add(maxBinding);

        var closeBinding = new CommandBinding { Command = AppWindowCloseCommand };
        closeBinding.Executed += CloseBinding_Executed;
        closeBinding.CanExecute += CloseBinding_CanExecute;
        CommandBindings.Add(closeBinding);

        var osVersionInfo = new OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)) };
        if (RtlGetVersion(ref osVersionInfo) != 0)
        {

        }
        else
        {
            switch (osVersionInfo.MajorVersion)
            {
                case 10:
                    WindowsThemeStyle = WindowsThemeVersion.Current;
                    break;
                case 6:
                    WindowsThemeStyle = osVersionInfo.MinorVersion switch
                    {
                        2 or 3 => WindowsThemeVersion.Aero2,
                        _ => WindowsThemeVersion.Aero,
                    };
                    break;
            }
        }
        SourceInitialized += AppWindow_SourceInitialized;

        FocusManager.SetFocusedElement(this, this);
    }

    private void AppWindow_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (Keyboard.FocusedElement != this && e.ClickCount < 2)
        {
            FocusManager.SetFocusedElement(this, this);
        }
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        Window? target = null;
        
        foreach (Window window in Application.Current.Windows)
        {
            if (new WindowInteropHelper(window).Handle == hwnd) target = window;
        }
        if (target is not null)
        {
            switch (msg)
            {
                case 0x86:

                    if (lParam == new IntPtr(0))
                    {
                        if (wParam == new IntPtr(1))
                        {
                            if (target is DialogWindow dialog) dialog.FlashWindow();
                        }
                        if (wParam == new IntPtr(0))
                        {
                            if (target is DialogWindow dialog) dialog.FlashWindow();
                        }
                    }
                    break;
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam, target);
                    handled = true;
                    break;
                /*case 0xF120:
                    target.WindowState = WindowState.Normal;
                    handled = true;
                    break;
                case 0xF020:
                    target.WindowState = WindowState.Minimized;
                    handled = true;
                    break;*/
                case 0x0214:
                    WmSizing(hwnd, lParam, target);
                    handled = true;
                    break;
                default:
                    break;
            }
        }
        return IntPtr.Zero;
    }

    private void AppWindow_SourceInitialized(object? sender, EventArgs e)
    {
        SourceInitialized -= AppWindow_SourceInitialized;
        InvalidateMeasure();

        HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source.AddHook(new HwndSourceHook(WndProc));
    }

    private static void WmSizing(IntPtr hwnd, IntPtr lParam, Window target)
    {
        RECT size = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
        if (target is null) return;
        Matrix scale = PresentationSource.FromVisual(target).CompositionTarget.TransformToDevice;
        int min_width = Convert.ToInt32(target.MinWidth * scale.M11);
        int min_height = Convert.ToInt32(target.MinHeight * scale.M22);
        if (min_width > size.Width) size.right = size.left + min_width;
        if (min_height > size.Height) size.bottom = size.top + min_height;
        Marshal.StructureToPtr(size, lParam, true);
    }

    private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam, Window target)
    {
        if (Marshal.PtrToStructure(lParam, typeof(MINMAXINFO)) is not MINMAXINFO mmi || target is null) return;

        int MONITOR_DEFAULTTONEAREST = 0x00000002;
        IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

        if (monitor != IntPtr.Zero)
        {
            MONITORINFO monitorInfo = new();
            GetMonitorInfo(monitor, monitorInfo);
            RECT rcWorkArea = monitorInfo.rcWork;
            RECT rcMonitorArea = monitorInfo.rcMonitor;
            mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
            mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
            mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
            mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            Matrix scale = PresentationSource.FromVisual(target).CompositionTarget.TransformToDevice;
            int min_width = Convert.ToInt32(target.MinWidth * scale.M11);
            int min_height = Convert.ToInt32(target.MinHeight * scale.M22);
            mmi.ptMinTrackSize.x = min_width;
            mmi.ptMinTrackSize.y = min_height;
        }

        Marshal.StructureToPtr(mmi, lParam, true);
    }


    [SecurityCritical]
    [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [LibraryImport("User32")]
    internal static partial IntPtr MonitorFromWindow(IntPtr handle, int flags);


    [StructLayout(LayoutKind.Sequential)]
    internal struct OSVERSIONINFOEX
    {
        internal int OSVersionInfoSize;
        internal int MajorVersion;
        internal int MinorVersion;
        internal int BuildNumber;
        internal int PlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string CSDVersion;
        internal ushort ServicePackMajor;
        internal ushort ServicePackMinor;
        internal short SuiteMask;
        internal byte ProductType;
        internal byte Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    { 
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor = new();
        public RECT rcWork = new();
        public int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public static readonly RECT Empty = new();

        public int Width
        {
            get { return Math.Abs(right - left); } 
        }

        public int Height
        {
            get { return bottom - top; }
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(RECT rcSrc)
        {
            this.left = rcSrc.left;
            this.top = rcSrc.top;
            this.right = rcSrc.right;
            this.bottom = rcSrc.bottom;
        }

        public bool IsEmpty
        {
            get
            {
                return left >= right || top >= bottom;
            }
        }

        public override string ToString()
        {
            if (this == Empty) { return "RECT {Empty}"; }
            return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
        }

        public override bool Equals(object? obj)
        {
            if (obj is RECT r) { return (this == r); }
            return false;
        }

        public override int GetHashCode()
        {
            return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
        }

        public static bool operator ==(RECT rect1, RECT rect2)
        {
            return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
        }

        public static bool operator !=(RECT rect1, RECT rect2)
        {
            return !(rect1 == rect2);
        }


    }


    public static readonly DependencyProperty WindowResizeBorderThicknessProperty = DependencyProperty.Register("WindowResizeBorderThickness", typeof(Thickness), typeof(AppWindow), new PropertyMetadata(SystemParameters.WindowResizeBorderThickness, OnWindowResizeBorderThicknessChanged));

    public Thickness WindowResizeBorderThickness
    {
        get
        {
            return (Thickness)GetValue(WindowResizeBorderThicknessProperty);
        }
        set
        {
            SetValue(WindowResizeBorderThicknessProperty, value);
        }
    }

    public static void OnWindowResizeBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (WindowChrome.GetWindowChrome((Window)d) is WindowChrome chrome)
        {
            chrome.ResizeBorderThickness = (Thickness)e.NewValue;
        }
    }

    public static readonly DependencyProperty WindowBackgroundProperty = DependencyProperty.Register("WindowBackground", typeof(Brush), typeof(AppWindow));

    public Brush WindowBackground
    {
        get
        {
            return (Brush)GetValue(WindowBackgroundProperty);
        }
        set
        {
            SetValue(WindowBackgroundProperty, value);
        }
    }

    public static readonly DependencyProperty WindowIconProperty = DependencyProperty.Register("WindowIcon", typeof(object), typeof(AppWindow));

    public object WindowIcon
    {
        get
        {
            return GetValue(WindowIconProperty);
        }
        set
        {
            SetValue(WindowIconProperty, value);
        }
    }

    public static readonly DependencyProperty HideTitleProperty = DependencyProperty.Register("HideTitle", typeof(Boolean), typeof(AppWindow));

    public Boolean HideTitle
    {
        get
        {
            return (Boolean)GetValue(HideTitleProperty);
        }
        set
        {
            SetValue(HideTitleProperty, value);
        }
    }

    public static readonly DependencyProperty WindowsThemeStyleProperty = DependencyProperty.Register("WindowsThemeStyle", typeof(WindowsThemeVersion), typeof(AppWindow), new PropertyMetadata(WindowsThemeVersion.Aero));

    public WindowsThemeVersion WindowsThemeStyle
    {
        get
        {
            return (WindowsThemeVersion)GetValue(WindowsThemeStyleProperty);
        }
        private set
        {
            SetValue(WindowsThemeStyleProperty, value);
        }
    }

    public static readonly DependencyProperty MenuBarProperty = DependencyProperty.Register("MenuBar", typeof(object), typeof(AppWindow));

    public object MenuBar
    {
        get
        {
            return GetValue(MenuBarProperty);
        }
        set
        {
            SetValue(MenuBarProperty, value);
        }
    }

    public static readonly DependencyProperty StatusColorProperty = DependencyProperty.Register("StatusColor", typeof(Brush), typeof(AppWindow));

    public Brush StatusColor
    {
        get
        {
            return (Brush)GetValue(StatusColorProperty);
        }
        set
        {
            SetValue(StatusColorProperty, value);
        }
    }

    public static readonly DependencyProperty TopStatusBlockProperty = DependencyProperty.Register("TopStatusBlock", typeof(object), typeof(AppWindow));

    public object TopStatusBlock
    {
        get
        {
            return GetValue(TopStatusBlockProperty);
        }
        set
        {
            SetValue(TopStatusBlockProperty, value);
        }
    }

    public static readonly DependencyProperty StatusBarProperty = DependencyProperty.Register("StatusBar", typeof(object), typeof(AppWindow));

    public object StatusBar
    {
        get
        {
            return GetValue(StatusBarProperty);
        }
        set
        {
            SetValue(StatusBarProperty, value);
        }
    }

    public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register("CaptionHeight", typeof(double), typeof(AppWindow), new(26d, OnCaptionHeightChanged));

    public double CaptionHeight
    {
        get
        {
            return (double)GetValue(CaptionHeightProperty);
        }
        set
        {
            SetValue(CaptionHeightProperty, value);
        }
    }
    public static void OnCaptionHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AppWindow window)
        {
            WindowChrome wc = WindowChrome.GetWindowChrome(window);
            wc.CaptionHeight = (double)e.NewValue;
        }

    }

    private void MinBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is AppWindow w && w.ResizeMode == ResizeMode.NoResize) e.CanExecute = false;
        else e.CanExecute = true;
    }

    private void MaxBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is AppWindow w && w.ResizeMode == ResizeMode.NoResize) e.CanExecute = false;
        else e.CanExecute = true;
    }

    private void CloseBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void MinBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if(sender is AppWindow w) w.WindowState = WindowState.Minimized;
    }

    private void MaxBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if(sender is AppWindow w && e.Source is AppWindow s) w.WindowState = s.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if (sender is AppWindow w) w.Close();
    }

    public static RoutedCommand AppWindowMinimizeCommand { get; } = new RoutedCommand("AppWindowMinimizeCommand", typeof(AppWindow));
    public static RoutedCommand AppWindowMaximizeCommand { get; } = new RoutedCommand("AppWindowMaximizeCommand", typeof(AppWindow));
    public static RoutedCommand AppWindowCloseCommand { get; } = new RoutedCommand("AppWindowCloseCommand", typeof(AppWindow));

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        MouseDown -= AppWindow_MouseDown;
    }
}
