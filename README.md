# rrebuffo.BaseUISupport

UI framework for WPF desktop apps with a special focus on CasparCG clients.  
It is the result of many years of work and iteration, and it is pretty much a work in prorgess.

## Main features

 - Unified UI and color scheme with light and dark modes.
 - Compact windows and dialogs with custom areas for status items, menus and controls.
 - Standarized buttons, toggles, labels, progress indicators and other controls.
 - Rich tab controls and tabbed panels with support for closing tabs, pinned tabs and buttons.
 - Custom behaviors for lists and other control boxes, supporting drag and drop, focus and other behaviors.
 - Counters support.
 - JSON settings library for apps.
 - App initialization (checks for single instance and arguments) and splash windows.
 - Multiple value converters compatible with binding and multibinding.

## Installation

 - Create a new C# WPF Application project in Visual Studio 2022 targeting .NET 8 Desktop
 - Add the nuget package '**rrebuffo.BaseUISupport**'
 - Open App.xaml and append the following resources inside the `<Application.Resources/>` tag:

            <ResourceDictionary>
                <ResourceDictionary.MergedDictionaries>
                    <ResourceDictionary x:Name="Colors" Source="pack://application:,,,/BaseUISupport;component/Styles/DarkColors.xaml"/>
                    <ResourceDictionary Source="pack://application:,,,/BaseUISupport;component/Styles/ControlTemplates.xaml"/>
                    <ResourceDictionary Source="pack://application:,,,/BaseUISupport;component/Styles/Icons.xaml"/>
                </ResourceDictionary.MergedDictionaries>
            </ResourceDictionary>
 - Open App.xaml.cs and add the constructor:  
   *(This will change the text selection to solid colors instead of the semi-transparent selection that WPF defaults to)*

        public App() : base()
        {
            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
        }

   If the goal is to have a single instance of the application, add override the `OnStartup` method:
   
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _ = AppInitHelper.SingleInstanceCheck("UNIQUE_NAME_FOR_THIS_APP");
        }
   
   Then add the following event handler to the window that should be activated when launching another instance:
      
        using BaseUISupport.Helpers;
   
        public MainWindow()
        {
            InitializeComponent();
            AppInitHelper.RequestActivate += ActivateSingleInstance;
        }

        private void ActivateSingleInstance(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                WindowState = WindowState.Normal;
                Topmost = true;
                Activate();
                Topmost = false;
            });
        }

  - Convert windows to AppWindow or DialogWindow:
    - Add namespace to both XAML and C# files:  
      `xmlns:ui="clr-namespace:BaseUISupport.Controls;assembly=BaseUISupport"`  
      `using BaseUISupport.Controls`
    - Change base class to `ui:AppWindow` or `ui:DialogWindow` in XAML and extend `AppWindow` or `DialogWindow` in C# files.
