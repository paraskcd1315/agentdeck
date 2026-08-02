using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.DesignSystem.Foundation;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace AgentDeck.Shell;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SystemBackdrop = new AdAcrylicBackdrop { Kind = DesktopAcrylicKind.Thin };

        var theme = AppServices.Config.Theme;
        AppGradient.Fill = AppBackgroundBrush.Build(theme);
        AppGradient.Opacity = AppBackgroundBrush.Opacity(theme);

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        Title = AppServices.Strings.Get(StringKeys.WindowTitle);
        AppTitleBar.Title = Title;

        Panels.ButtonInvoked += OnPanelButtonInvoked;
        Activated += OnActivated;
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState != WindowActivationState.Deactivated)
        {
            Terminal.TakeFocus();
        }
    }

    private async void OnPanelButtonInvoked(object? sender, PanelButton button)
    {
        await Terminal.InjectAsync(button.Prompt);
        Terminal.TakeFocus();
    }
}
