using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Diagnostics;
using System.Security;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;

namespace BaseUISupport.Controls;

[TemplatePart(Name = "WindowBorder", Type = typeof(Border))]
[TemplatePart(Name = "Title", Type = typeof(TextBlock))]

public partial class DialogWindow : Window, INotifyPropertyChanged, IDisposable
{
    public DialogWindow()
    {

        bool style_is_set = false;

        while (!style_is_set)
        {
            try
            {
                Style = (Style)FindResource("DialogWindowStyle");
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

        MouseDown += DialogWindow_MouseDown;

        var minBinding = new CommandBinding { Command = DialogWindowMinimizeCommand };
        minBinding.Executed += MinBinding_Executed;
        minBinding.CanExecute += MinBinding_CanExecute;
        CommandBindings.Add(minBinding);

        var maxBinding = new CommandBinding { Command = DialogWindowMaximizeCommand };
        maxBinding.Executed += MaxBinding_Executed;
        maxBinding.CanExecute += MaxBinding_CanExecute;
        CommandBindings.Add(maxBinding);

        var closeBinding = new CommandBinding { Command = DialogWindowCloseCommand };
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


        SourceInitialized += DialogWindow_SourceInitialized;
    }

    DateTime flashThreshold = DateTime.MinValue;

    public void FlashWindow()
    {
        if(DateTime.Now - flashThreshold > TimeSpan.FromMilliseconds(200))
        {
            flashThreshold = DateTime.Now;
            return;
        }

        if (Template.FindName("WindowBorder", this) is Border border && Template.FindName("Title", this) is TextBlock title)
        {
            var border_value = border.Opacity;
            var title_value = title.Opacity;
            
            border.Opacity = border_value / 2;
            title.Opacity = title_value / 2;

            Timer timer = new(new TimerCallback((object? state) =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    border.Opacity = border_value;
                    title.Opacity = title_value;
                }));
                
            }), null, 50, 0);
        }

        flashThreshold = DateTime.MinValue;
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

    private void DialogWindow_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (Keyboard.FocusedElement != this && e.ClickCount < 2)
        {
            FocusManager.SetFocusedElement(this, this);
        }
    }

    private void DialogWindow_SourceInitialized(object? sender, EventArgs e)
    {
        SourceInitialized -= DialogWindow_SourceInitialized;
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

    [LibraryImport("user32.dll", SetLastError = false)]
    private static partial IntPtr GetDesktopWindow();
    
    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr SetActiveWindow(IntPtr hWnd);

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

    public static readonly DependencyProperty ResultProperty = DependencyProperty.Register("Result", typeof(MessageBoxResult), typeof(DialogWindow), new PropertyMetadata(MessageBoxResult.None));
    public MessageBoxResult Result
    {
        get => (MessageBoxResult)GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(MessageBoxImage), typeof(DialogWindow), new PropertyMetadata(MessageBoxImage.None));
    public MessageBoxImage Image
    {
        get => (MessageBoxImage)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    public static readonly DependencyProperty ButtonProperty = DependencyProperty.Register("Button", typeof(MessageBoxButton), typeof(DialogWindow), new PropertyMetadata(MessageBoxButton.OK));
    public MessageBoxButton Button
    {
        get => (MessageBoxButton)GetValue(ButtonProperty);
        set => SetValue(ButtonProperty, value);
    }

    public static readonly DependencyProperty FieldsProperty = DependencyProperty.Register("Fields", typeof(EditBoxField[]), typeof(DialogWindow), new PropertyMetadata(Array.Empty<EditBoxField>()));
    public EditBoxField[] Fields
    {
        get => (EditBoxField[])GetValue(FieldsProperty);
        set => SetValue(FieldsProperty, value);
    }

    public static readonly DependencyProperty DialogIconProperty = DependencyProperty.Register("DialogIcon", typeof(object), typeof(DialogWindow));
    public object DialogIcon
    {
        get => GetValue(DialogIconProperty);
        set => SetValue(DialogIconProperty, value);
    }

    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(DialogWindow));
    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(object), typeof(DialogWindow));
    public object Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty WindowsThemeStyleProperty = DependencyProperty.Register("WindowsThemeStyle", typeof(WindowsThemeVersion), typeof(DialogWindow), new PropertyMetadata(WindowsThemeVersion.Aero));
    public WindowsThemeVersion WindowsThemeStyle
    {
        get => (WindowsThemeVersion)GetValue(WindowsThemeStyleProperty);
        private set => SetValue(WindowsThemeStyleProperty, value);
    }

    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register("Options", typeof(MessageBoxOptions), typeof(DialogWindow), new PropertyMetadata(MessageBoxOptions.None));
    public MessageBoxOptions Options
    {
        get => (MessageBoxOptions)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }


    public static RoutedCommand DialogWindowMaximizeCommand { get; } = new RoutedCommand("DialogWindowMaximizeCommand", typeof(DialogWindow));
    private void MaxBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }
    private void MaxBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if (sender is DialogWindow w) w.WindowState = ((DialogWindow)sender).WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }


    public static RoutedCommand DialogWindowMinimizeCommand { get; } = new RoutedCommand("DialogWindowMinimizeCommand", typeof(DialogWindow));
    private void MinBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }
    private void MinBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if (sender is DialogWindow w) w.WindowState = WindowState.Minimized;
    }


    public static RoutedCommand DialogWindowCloseCommand { get; } = new RoutedCommand("DialogWindowCloseCommand", typeof(DialogWindow));
    private void CloseBinding_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }
    private void CloseBinding_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        if (sender is DialogWindow w) w.Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        MouseDown -= DialogWindow_MouseDown;
        Debug.WriteLine("Dialog disposed");
    }
}
