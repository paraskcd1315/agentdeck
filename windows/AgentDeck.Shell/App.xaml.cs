using AgentDeck.Shell.Data.Diagnostics;

using Microsoft.UI.Xaml;

namespace AgentDeck.Shell;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, args) => CrashLog.Write(args.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                CrashLog.Write(exception);
            }
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
