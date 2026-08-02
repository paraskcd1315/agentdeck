using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace AgentDeck.Shell.Presentation.Terminal.Components;

public sealed partial class TerminalTabStrip : UserControl
{
    private const string StrokeKey = "AdStrokeBrush";
    private const string StrokeBrandKey = "AdStrokeBrandBrush";
    private const string TextKey = "AdTextBrush";
    private const string TextMutedKey = "AdText2Brush";

    private IReadOnlyList<ShellProfile> _profiles = [];

    public TerminalTabStrip()
    {
        InitializeComponent();
        AddButton.Content = AppServices.Strings.Get(StringKeys.TerminalTabNew);
    }

    public event EventHandler<TerminalTab>? TabSelected;

    public event EventHandler<TerminalTab>? TabClosed;

    public event EventHandler<ShellProfile>? ProfileRequested;

    public void Render(
        IReadOnlyList<TerminalTab> tabs,
        TerminalTab? active,
        IReadOnlyList<ShellProfile> profiles)
    {
        _profiles = profiles;
        TabHost.Children.Clear();

        foreach (var tab in tabs)
        {
            TabHost.Children.Add(BuildTab(tab, ReferenceEquals(tab, active)));
        }
    }

    private Button BuildTab(TerminalTab tab, bool active)
    {
        var label = new TextBlock
        {
            Text = tab.Title,
            Foreground = PanelResources.Brush(active ? TextKey : TextMutedKey),
        };

        var close = new Button
        {
            Content = AppServices.Strings.Get(StringKeys.TerminalTabClose),
            Background = null,
            BorderThickness = new Thickness(0),
            Foreground = PanelResources.Brush(TextMutedKey),
            AllowFocusOnInteraction = false,
            IsTabStop = false,
        };
        close.Click += (_, _) => TabClosed?.Invoke(this, tab);

        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(label);
        content.Children.Add(close);

        var button = new Button
        {
            Content = content,
            Background = null,
            BorderBrush = PanelResources.Brush(active ? StrokeBrandKey : StrokeKey),
            BorderThickness = new Thickness(1),
            AllowFocusOnInteraction = false,
            IsTabStop = false,
        };
        button.Click += (_, _) => TabSelected?.Invoke(this, tab);

        return button;
    }

    private void OnAddClick(object sender, RoutedEventArgs args)
    {
        if (_profiles.Count == 1)
        {
            ProfileRequested?.Invoke(this, _profiles[0]);
            return;
        }

        var menu = new MenuFlyout { Placement = FlyoutPlacementMode.Bottom };

        foreach (var profile in _profiles)
        {
            var item = new MenuFlyoutItem { Text = profile.Name ?? Constants.Shell.DefaultProfileName };
            item.Click += (_, _) => ProfileRequested?.Invoke(this, profile);
            menu.Items.Add(item);
        }

        menu.ShowAt(AddButton);
    }
}
