using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Security;
using System.Windows.Media;
using System.Threading;
using BaseUISupport.Helpers;

namespace BaseUISupport.Controls;

public partial class FullscreenWindow : Window, INotifyPropertyChanged
{

    public FullscreenWindow()
    {

        bool style_is_set = false;

        while (!style_is_set)
        {
            try
            {
                Style = (Style)FindResource("FullscreenWindowStyle");
                style_is_set = true;
            }
            catch (Exception)
            {
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/BaseUISupport;component/Styles/DarkColors.xaml", UriKind.Relative)));
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/BaseUISupport;component/Styles/ControlTemplates.xaml", UriKind.Relative)));
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/BaseUISupport;component/Styles/Icons.xaml", UriKind.Relative)));
            }

            Thread.Sleep(10);
        }

        MouseDown += FullscreenWindow_MouseDown;
        SourceInitialized += FullscreenWindow_SourceInitialized;
        SizeChanged += FullscreenWindow_SizeChanged;
    }

    private void FullscreenWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
            return;
        }
        //WindowState = WindowState.Normal;
    }

    private void FullscreenWindow_SourceInitialized(object? sender, EventArgs e)
    {
        InvalidateMeasure();

        HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source.AddHook(new HwndSourceHook(WndProc));

        this.EnableHorizontalWheel();
    }

    private void FullscreenWindow_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (Keyboard.FocusedElement != this && e.ClickCount < 2)
        {
            FocusManager.SetFocusedElement(this, this);
        }
    }

    const int WM_SYSCOMMAND = 0x0112;
    const int SC_SIZE = 0xF000;
    const int SC_MOVE = 0xF010;
    const int SC_MINIMIZE = 0xF020;
    const int SC_MAXIMIZE = 0xF030;
    const int SC_NEXTWINDOW = 0xF040;
    const int SC_PREVWINDOW = 0xF050;
    const int SC_CLOSE = 0xF060;
    const int SC_KEYMENU = 0xF100;
    const int SC_RESTORE = 0xF120;
    const int SC_TASKLIST = 0xF130;

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        Window? target = null;

        foreach (Window window in Application.Current.Windows)
        {
            if (new WindowInteropHelper(window).Handle == hwnd) target = window;
        }
        switch (msg)
        {
            case 0x0024:
                if(target is null) break;
                WmGetMinMaxInfo(hwnd, lParam, target);
                handled = true;
                break;
            case WM_SYSCOMMAND:
                switch (wParam.ToInt32() & 0xfff0)
                {
                    case SC_SIZE:
                    case SC_MOVE:
                    case SC_MINIMIZE:
                    case SC_MAXIMIZE:
                    case SC_NEXTWINDOW:
                    case SC_PREVWINDOW:
                    case SC_CLOSE:
                    case SC_KEYMENU:
                    case SC_RESTORE:
                    case SC_TASKLIST:
                        handled = true;
                        break;
                }
                break;
        }
        return IntPtr.Zero;
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
            RECT rcMonitorArea = monitorInfo.rcMonitor;
            mmi.ptMaxPosition.x = rcMonitorArea.left;
            mmi.ptMaxPosition.y = rcMonitorArea.top;
            mmi.ptMaxSize.x = Math.Abs(rcMonitorArea.right - rcMonitorArea.left);
            mmi.ptMaxSize.y = Math.Abs(rcMonitorArea.bottom - rcMonitorArea.top);
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

    public static readonly DependencyProperty WindowIconProperty = DependencyProperty.Register("WindowIcon", typeof(object), typeof(FullscreenWindow));

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

    public static readonly DependencyProperty HideTitleProperty = DependencyProperty.Register("HideTitle", typeof(Boolean), typeof(FullscreenWindow));

    public object HideTitle
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

    public static readonly DependencyProperty WindowsThemeStyleProperty = DependencyProperty.Register("WindowsThemeStyle", typeof(WindowsThemeVersion), typeof(FullscreenWindow), new PropertyMetadata(WindowsThemeVersion.Aero));

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

    public static readonly DependencyProperty MenuBarProperty = DependencyProperty.Register("MenuBar", typeof(object), typeof(FullscreenWindow));

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

    public static readonly DependencyProperty StatusColorProperty = DependencyProperty.Register("StatusColor", typeof(Brush), typeof(FullscreenWindow));

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

    public static readonly DependencyProperty TopStatusBlockProperty = DependencyProperty.Register("TopStatusBlock", typeof(object), typeof(FullscreenWindow));

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

    public static readonly DependencyProperty StatusBarProperty = DependencyProperty.Register("StatusBar", typeof(object), typeof(FullscreenWindow));

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

    private void MinBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void MaxBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CloseBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void MinBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if(sender is FullscreenWindow w) w.WindowState = WindowState.Minimized;
    }

    private void MaxBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if(sender is FullscreenWindow w && e.Source is FullscreenWindow s) w.WindowState = s.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if (sender is FullscreenWindow w) w.Close();
    }

    public static RoutedCommand FullscreenWindowMinimizeCommand { get; } = new RoutedCommand("FullscreenWindowMinimizeCommand", typeof(FullscreenWindow));
    public static RoutedCommand FullscreenWindowMaximizeCommand { get; } = new RoutedCommand("FullscreenWindowMaximizeCommand", typeof(FullscreenWindow));
    public static RoutedCommand FullscreenWindowCloseCommand { get; } = new RoutedCommand("FullscreenWindowCloseCommand", typeof(FullscreenWindow));

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
