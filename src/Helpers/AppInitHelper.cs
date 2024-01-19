using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using C = BaseUISupport.Controls;

namespace BaseUISupport.Helpers;


public static class AppInitHelper
{
    private static Mutex? Mutex;
    private static EventWaitHandle? eventWaitHandle;
    private static MemoryMappedFile? file;
    private static string? EventName;

    public static List<string> Args { get; } = [];

    public static string[] SingleInstanceCheck(string appName)
    {
        string[] args = Environment.GetCommandLineArgs();
        
        Mutex = new Mutex(true, appName + "_Mutex", out bool owned);
        EventName = appName + "_Event";
        eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
        GC.KeepAlive(Mutex);
        if (owned)
        {
            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
            CreateMutexAndRun(appName, args);
        }
        else
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                MessageBoxResult result = C.MessageBox.Show("Are you sure you want to run a new instance?", "Instance already running", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    return args;
                }
                else
                {
                    PassArgumentsAndClose(appName, args);
                }
            }
            else
            {
                PassArgumentsAndClose(appName, args);
            }
            
        }

        return args;
    }

    private static void CreateMutexAndRun(string appName, string[] args)
    {
        if (args.Length > 1) Args.Add(args[1]);
        file = MemoryMappedFile.CreateNew(appName + "_Map", 10000);
        GC.KeepAlive(file);
        using (MemoryMappedViewStream stream = file.CreateViewStream(0, 10))
        {
            BinaryWriter writer = new(stream);
            writer.Write("");
        }
        if (eventWaitHandle is null) return;
        var thread = new Thread(() =>
        {
            while (eventWaitHandle.WaitOne())
            {
                Args.Clear();
                using (MemoryMappedViewStream stream = file.CreateViewStream(10, 0))
                {
                    BinaryReader reader = new(stream);
                    foreach (string a in reader.ReadString().Split('\n')) Args.Add(a);
                }
                OnRequestActivate();
            }
        })
        {
            IsBackground = true
        };
        thread.Start();
    }

    private static void PassArgumentsAndClose(string appName, string[] args)
    {
        try
        {
            Mutex MapMutex = new(false, appName + "_MapMutex");
            if (MapMutex.WaitOne())
            {
                try
                {
                    using MemoryMappedFile file = MemoryMappedFile.OpenExisting(appName + "_Map");
                    using MemoryMappedViewStream stream = file.CreateViewStream(10, 0);
                    BinaryWriter writer = new(stream);
                    var m = "";
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i > 1) m += "\n";
                        if (i > 0) m += args[i];
                    }
                    writer.Write(m);
                }
                catch (FileNotFoundException)
                {
                }
                MapMutex.ReleaseMutex();
            }
        }
        catch (Exception)
        {
        }
        eventWaitHandle?.Set();
        Process.GetCurrentProcess().Kill();
    }

    public static event EventHandler? RequestActivate;
    private static void OnRequestActivate()
    {
        RequestActivate?.Invoke(null, new EventArgs());
    }
}
