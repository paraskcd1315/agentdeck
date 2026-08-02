using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Terminal.Components;

public sealed partial class TerminalTabStrip : UserControl
{
    private const string StrokeKey = "AdStrokeBrush";
    private const string GlassKey = "AdGlassBrush";
    private const string TextKey = "AdTextBrush";
    private const string TextMutedKey = "AdText2Brush";
    private const string TextFaintKey = "AdText3Brush";
    private const string IdleDotKey = "AdStateIdleBrush";
    private const string ActiveDotKey = "AdStateWorkingBrush";
    private const string FontKey = "AdFontUi";
    private const string SizeKey = "AdUiSize";
    private const string CaptionSizeKey = "AdCaptionSize";

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
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(BuildDot(active));
        content.Children.Add(BuildLabel(tab, active));
        content.Children.Add(BuildClose(tab));

        var button = new Button
        {
            Content = content,
            Padding = TerminalTabMetrics.TabPadding,
            CornerRadius = TerminalTabMetrics.TabRadius,
            Background = active ? PanelResources.Brush(GlassKey) : null,
            BorderBrush = active ? PanelResources.Brush(StrokeKey) : null,
            BorderThickness = active ? TerminalTabMetrics.TabBorder : new Thickness(0),
            AllowFocusOnInteraction = false,
            IsTabStop = false,
        };

        button.Click += (_, _) => TabSelected?.Invoke(this, tab);
        return button;
    }

    private static Border BuildDot(bool active) => new()
    {
        Width = TerminalTabMetrics.DotSize,
        Height = TerminalTabMetrics.DotSize,
        CornerRadius = new CornerRadius(TerminalTabMetrics.DotSize / 2),
        VerticalAlignment = VerticalAlignment.Center,
        Background = PanelResources.Brush(active ? ActiveDotKey : IdleDotKey),
    };

    private static TextBlock BuildLabel(TerminalTab tab, bool active) => new()
    {
        Text = tab.Title,
        Margin = TerminalTabMetrics.ContentGap,
        FontFamily = PanelResources.Font(FontKey),
        FontSize = PanelResources.Size(SizeKey),
        FontWeight = active ? FontWeights.SemiBold : FontWeights.Medium,
        Foreground = PanelResources.Brush(active ? TextKey : TextMutedKey),
        VerticalAlignment = VerticalAlignment.Center,
    };

    private Button BuildClose(TerminalTab tab)
    {
        var close = new Button
        {
            Content = AppServices.Strings.Get(StringKeys.TerminalTabClose),
            Margin = TerminalTabMetrics.ClosePadding,
            Padding = new Thickness(0),
            Background = null,
            BorderThickness = new Thickness(0),
            FontFamily = PanelResources.Font(FontKey),
            FontSize = PanelResources.Size(CaptionSizeKey),
            FontWeight = FontWeights.SemiBold,
            Foreground = PanelResources.Brush(TextFaintKey),
            VerticalAlignment = VerticalAlignment.Center,
            AllowFocusOnInteraction = false,
            IsTabStop = false,
        };

        close.Click += (_, _) => TabClosed?.Invoke(this, tab);
        return close;
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
