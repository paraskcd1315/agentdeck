using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Windows.ApplicationModel.DataTransfer;

namespace AgentDeck.Shell.Presentation.Terminal.Components;

public sealed partial class TerminalTabStrip : UserControl
{
    private const string StrokeKey = "AdStrokeBrush";
    private const string GlassKey = "AdGlassBrush";
    private const string HoverKey = "AdHoverBrush";
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
        ToolTipService.SetToolTip(AddButton, AppServices.Strings.Get(StringKeys.TerminalTabNewTooltip));
        SizeChanged += (_, _) => ShowOverflowEdge();
    }

    public event EventHandler<TerminalTab>? TabSelected;

    public event EventHandler<TerminalTab>? TabClosed;

    public event EventHandler<ShellProfile>? ProfileRequested;

    public event EventHandler? PaneDroppedOnStrip;

    private void OnStripDragOver(object sender, DragEventArgs args)
    {
        args.AcceptedOperation = DataPackageOperation.Move;
        args.DragUIOverride.IsGlyphVisible = false;
        args.Handled = true;
    }

    private void OnStripDrop(object sender, DragEventArgs args)
    {
        args.Handled = true;
        PaneDroppedOnStrip?.Invoke(this, EventArgs.Empty);
    }

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

        DispatcherQueue.TryEnqueue(ShowOverflowEdge);
    }

    private void ShowOverflowEdge() =>
        OverflowEdge.Visibility = TabScroller.ScrollableWidth > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

    private void OnStripWheel(object sender, PointerRoutedEventArgs args)
    {
        var delta = args.GetCurrentPoint(TabScroller).Properties.MouseWheelDelta;
        if (delta == 0)
        {
            return;
        }

        args.Handled = true;
        TabScroller.ChangeView(TabScroller.HorizontalOffset - delta, null, null, true);
    }

    private Border BuildTab(TerminalTab tab, bool active)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(BuildDot(active));
        content.Children.Add(BuildLabel(tab, active));
        content.Children.Add(BuildClose(tab));

        var resting = active ? PanelResources.Brush(GlassKey) : Transparent();

        var surface = new Border
        {
            Child = content,
            Padding = TerminalTabMetrics.TabPadding,
            CornerRadius = TerminalTabMetrics.TabRadius,
            Background = resting,
            BorderBrush = active ? PanelResources.Brush(StrokeKey) : null,
            BorderThickness = active ? TerminalTabMetrics.TabBorder : new Thickness(0),
        };

        surface.PointerEntered += (_, _) => surface.Background = PanelResources.Brush(HoverKey);
        surface.PointerExited += (_, _) => surface.Background = resting;
        surface.PointerPressed += (_, args) =>
        {
            args.Handled = true;
            TabSelected?.Invoke(this, tab);
        };

        return surface;
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
        MaxWidth = TerminalTabMetrics.MaxLabelWidth,
        TextTrimming = TextTrimming.CharacterEllipsis,
        TextWrapping = TextWrapping.NoWrap,
        FontFamily = PanelResources.Font(FontKey),
        FontSize = PanelResources.Size(SizeKey),
        FontWeight = active ? FontWeights.SemiBold : FontWeights.Medium,
        Foreground = PanelResources.Brush(active ? TextKey : TextMutedKey),
        VerticalAlignment = VerticalAlignment.Center,
    };

    private Border BuildClose(TerminalTab tab)
    {
        var glyph = new TextBlock
        {
            Text = AppServices.Strings.Get(StringKeys.TerminalTabClose),
            FontFamily = PanelResources.Font(FontKey),
            FontSize = PanelResources.Size(CaptionSizeKey),
            FontWeight = FontWeights.SemiBold,
            Foreground = PanelResources.Brush(TextFaintKey),
        };

        ToolTipService.SetToolTip(glyph, AppServices.Strings.Get(StringKeys.TerminalTabCloseTooltip));

        var close = new Border
        {
            Child = glyph,
            Margin = TerminalTabMetrics.ClosePadding,
            Padding = TerminalTabMetrics.ClosePad,
            CornerRadius = TerminalTabMetrics.CloseRadius,
            Background = Transparent(),
            VerticalAlignment = VerticalAlignment.Center,
        };

        close.PointerEntered += (_, _) =>
        {
            close.Background = PanelResources.Brush(HoverKey);
            glyph.Foreground = PanelResources.Brush(TextKey);
        };

        close.PointerExited += (_, _) =>
        {
            close.Background = Transparent();
            glyph.Foreground = PanelResources.Brush(TextFaintKey);
        };

        close.PointerPressed += (_, args) =>
        {
            args.Handled = true;
            TabClosed?.Invoke(this, tab);
        };

        return close;
    }

    private static SolidColorBrush Transparent() => new(Microsoft.UI.Colors.Transparent);

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
