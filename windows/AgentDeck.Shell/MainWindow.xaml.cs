using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;

namespace AgentDeck.Shell;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        Title = AppServices.Strings.Get(StringKeys.WindowTitle);
        AppTitleBar.Title = Title;

        Panels.ButtonInvoked += OnPanelButtonInvoked;
    }

    private async void OnPanelButtonInvoked(object? sender, PanelButton button)
    {
        await Terminal.InjectAsync(button.Prompt);
    }
}
