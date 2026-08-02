using AgentDeck.Shell.Data.Localization;
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

        var strings = new ResourceStringProvider();
        Title = strings.Get(StringKeys.WindowTitle);
        AppTitleBar.Title = Title;
    }
}
