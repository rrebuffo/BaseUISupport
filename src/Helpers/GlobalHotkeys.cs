using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BaseUISupport.Helpers;

public class GlobalHotkeys
{
    private static HwndSource? Source;
    private static IntPtr Handle;

    [DllImport("User32.dll")]
    private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

    [DllImport("User32.dll")]
    private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);

    private const int HOTKEY_ID = 9000;

    public static void Initialize(Window window)
    {
        Handle = new WindowInteropHelper(window).Handle;
        Source = HwndSource.FromHwnd(Handle);
    }

    public static void Uninitialize()
    {
        if (Source is not HwndSource source) return;
        source.RemoveHook(HwndHook);
        Source = null;
    }

    public static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        switch (msg)
        {
            case WM_HOTKEY:
                int key = wParam.ToInt32();
                switch (key)
                {
                    case HOTKEY_ID:
                        OnKeyPressed(key, ref handled);
                        break;
                }
                break;
        }
        return IntPtr.Zero;
    }

    public static event EventHandler<KeyPressedEventArgs>? KeyPressed;
    protected static void OnKeyPressed(int key, ref bool handled) { KeyPressed?.Invoke(null, new KeyPressedEventArgs(key, ref handled)); }
}

public class KeyPressedEventArgs : EventArgs
{
    public int Hotkey { get; private set; }
    public bool Handled { get; set; }

    public KeyPressedEventArgs(int key, ref bool handled)
    {
        Hotkey = key;
        Handled = handled;
    }
}