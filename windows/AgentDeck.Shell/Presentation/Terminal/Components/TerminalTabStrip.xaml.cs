using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.DesignSystem.Foundation;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.System;

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
    private IReadOnlyList<TerminalTab> _order = [];
    private TerminalTab? _active;
    private TerminalTab? _dragging;
    private Border? _draggingSurface;

    public TerminalTabStrip()
    {
        InitializeComponent();
        AddButton.Content = AppServices.Strings.Get(StringKeys.TerminalTabNew);
        ToolTipService.SetToolTip(AddButton, AppServices.Strings.Get(StringKeys.TerminalTabNewTooltip));
        SizeChanged += (_, _) => Reflow();
        TabHost.SizeChanged += (_, _) => Reflow();
        TabScroller.ViewChanged += (_, _) => RegionsChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<TerminalTab>? TabSelected;

    public event EventHandler<TerminalTab>? TabClosed;

    public event EventHandler<ShellProfile>? ProfileRequested;

    public event EventHandler? PaneDroppedOnStrip;

    public event EventHandler<TerminalTab>? TabRenamed;

    public event EventHandler<TerminalTabMoveRequest>? TabMoved;

    public event EventHandler? RegionsChanged;

    public event EventHandler<TerminalTab?>? TabDragChanged;

    public event EventHandler<TerminalTab>? TabDroppedOutside;

    public event EventHandler? ExplorerRequested;

    public IReadOnlyList<Rect> InteractiveRegions(UIElement reference)
    {
        if (StripRoot.ActualWidth <= 0 || StripRoot.ActualHeight <= 0)
        {
            return [];
        }

        return [RectOf(StripRoot, reference)];
    }

    private static Rect RectOf(FrameworkElement element, UIElement reference)
    {
        var origin = element.TransformToVisual(reference).TransformPoint(new Point(0, 0));
        return new Rect(origin.X, origin.Y, element.ActualWidth, element.ActualHeight);
    }

    private void OnStripDragOver(object sender, DragEventArgs args)
    {
        args.AcceptedOperation = DataPackageOperation.Move;
        args.DragUIOverride.IsGlyphVisible = false;
        args.Handled = true;

        if (_draggingSurface is not { } surface)
        {
            return;
        }

        var current = TabHost.Children.IndexOf(surface);

        if (current < 0)
        {
            return;
        }

        var target = TabInsertPoints.Resolve(TabBounds(), args.GetPosition(StripRoot).X);
        var next = Math.Clamp(target > current ? target - 1 : target, 0, TabHost.Children.Count - 1);

        if (next != current)
        {
            TabHost.Children.Move((uint)current, (uint)next);
        }
    }

    private void Readopt()
    {
        if (_dragging is not { } tab)
        {
            return;
        }

        _draggingSurface = TabHost.Children
            .OfType<Border>()
            .FirstOrDefault(surface => ReferenceEquals(surface.Tag, tab));

        if (_draggingSurface is { } adopted)
        {
            adopted.Opacity = TerminalTabMetrics.GhostOpacity;
        }
    }

    private void OnStripDragLeave(object sender, DragEventArgs args)
    {
    }

    private void OnStripDrop(object sender, DragEventArgs args)
    {
        args.Handled = true;

        if (_dragging is not { } tab || _draggingSurface is not { } surface)
        {
            PaneDroppedOnStrip?.Invoke(this, EventArgs.Empty);
            return;
        }

        var index = TabHost.Children.IndexOf(surface);

        _dragging = null;
        _draggingSurface = null;
        surface.Opacity = 1;

        if (index < 0)
        {
            return;
        }

        TabMoved?.Invoke(this, new TerminalTabMoveRequest(tab, index));
    }

    private IReadOnlyList<Rect> TabBounds() =>
        [.. TabHost.Children.OfType<Border>().Select(BoundsOf)];

    private Rect BoundsOf(Border surface) => RectOf(surface, StripRoot);

    private async void OnTabDragStarting(UIElement sender, DragStartingEventArgs args)
    {
        if (sender is not Border { Tag: TerminalTab tab } surface)
        {
            args.Cancel = true;
            return;
        }

        _dragging = tab;
        _draggingSurface = surface;
        args.Data.RequestedOperation = DataPackageOperation.Move;
        args.Data.SetText(tab.StripTitle);

        var deferral = args.GetDeferral();

        try
        {
            if (await CaptureAsync(surface) is { } bitmap)
            {
                args.DragUI.SetContentFromSoftwareBitmap(bitmap);
            }

            TabDragChanged?.Invoke(this, tab);

            if (_draggingSurface is { } dragged)
            {
                dragged.Opacity = TerminalTabMetrics.GhostOpacity;
            }
        }
        finally
        {
            deferral.Complete();
        }
    }

    private static async Task<SoftwareBitmap?> CaptureAsync(Border surface)
    {
        var radius = surface.CornerRadius;
        var background = surface.Background;
        var border = surface.BorderThickness;

        surface.CornerRadius = TerminalTabMetrics.DragRadius;
        surface.Background = PanelResources.Brush(GlassKey);
        surface.BorderThickness = TerminalTabMetrics.DragBorder;
        surface.BorderBrush = PanelResources.Brush(StrokeKey);

        try
        {
            var render = new RenderTargetBitmap();
            await render.RenderAsync(surface);

            return SoftwareBitmap.CreateCopyFromBuffer(
                await render.GetPixelsAsync(),
                BitmapPixelFormat.Bgra8,
                render.PixelWidth,
                render.PixelHeight,
                BitmapAlphaMode.Premultiplied);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            surface.CornerRadius = radius;
            surface.Background = background;
            surface.BorderThickness = border;
        }
    }

    private void OnTabDropCompleted(UIElement sender, DropCompletedEventArgs args)
    {
        if (sender is Border surface)
        {
            surface.Opacity = 1;
        }

        var pending = _dragging;

        _dragging = null;
        _draggingSurface = null;
        TabDragChanged?.Invoke(this, null);

        if (pending is null)
        {
            return;
        }

        Render(_order, _active, _profiles);

        if (args.DropResult == DataPackageOperation.None)
        {
            TabDroppedOutside?.Invoke(this, pending);
        }
    }

    public void Render(
        IReadOnlyList<TerminalTab> tabs,
        TerminalTab? active,
        IReadOnlyList<ShellProfile> profiles)
    {
        _profiles = profiles;
        _order = tabs;
        _active = active;
        TabHost.Children.Clear();

        foreach (var tab in tabs)
        {
            TabHost.Children.Add(BuildTab(tab, ReferenceEquals(tab, active)));
        }

        Readopt();
        DispatcherQueue.TryEnqueue(Reflow);
    }

    private void Reflow()
    {
        ShowOverflowEdge();
        RegionsChanged?.Invoke(this, EventArgs.Empty);
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
            Tag = tab,
            CanDrag = true,
            Padding = TerminalTabMetrics.TabPadding,
            CornerRadius = TerminalTabMetrics.TabRadius,
            Background = resting,
            BorderBrush = active ? PanelResources.Brush(StrokeKey) : null,
            BorderThickness = active ? TerminalTabMetrics.TabBorder : new Thickness(0),
        };

        surface.DragStarting += OnTabDragStarting;
        surface.DropCompleted += OnTabDropCompleted;
        surface.Loaded += (_, _) => AdMotion.SlideOnReposition(surface);

        surface.PointerEntered += (_, _) => surface.Background = PanelResources.Brush(HoverKey);
        surface.PointerExited += (_, _) => surface.Background = resting;
        surface.PointerPressed += (_, args) =>
        {
            args.Handled = true;

            if (args.GetCurrentPoint(surface).Properties.IsLeftButtonPressed)
            {
                TabSelected?.Invoke(this, tab);
            }
        };

        surface.RightTapped += (_, args) =>
        {
            args.Handled = true;
            ShowTabMenu(tab, surface);
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

    private TextBlock BuildLabel(TerminalTab tab, bool active)
    {
        var label = NewLabel(tab, active);
        label.DoubleTapped += (_, args) =>
        {
            args.Handled = true;
            BeginRename(tab, label);
        };

        return label;
    }

    private void ShowTabMenu(TerminalTab tab, Border surface)
    {
        var menu = new MenuFlyout { Placement = FlyoutPlacementMode.Bottom };

        menu.Items.Add(NewMenuItem(
            StringKeys.TerminalTabRename,
            () => RenameFrom(surface, tab)));

        menu.Items.Add(NewMenuItem(
            StringKeys.TerminalTabMoveToNewWindow,
            () => TabDroppedOutside?.Invoke(this, tab)));

        menu.Items.Add(new MenuFlyoutSeparator());

        menu.Items.Add(NewMenuItem(
            StringKeys.TerminalTabCloseCommand,
            () => TabClosed?.Invoke(this, tab)));

        menu.ShowAt(surface);
    }

    private static MenuFlyoutItem NewMenuItem(string key, Action invoked)
    {
        var item = new MenuFlyoutItem { Text = AppServices.Strings.Get(key) };
        item.Click += (_, _) => invoked();
        return item;
    }

    private void RenameFrom(Border surface, TerminalTab tab)
    {
        if (surface.Child is StackPanel host
            && host.Children.OfType<TextBlock>().FirstOrDefault() is { } label)
        {
            BeginRename(tab, label);
        }
    }

    private void BeginRename(TerminalTab tab, TextBlock label)
    {
        if (label.Parent is not StackPanel host)
        {
            return;
        }

        var index = host.Children.IndexOf(label);
        var editor = NewEditor(tab, label);
        var closed = false;

        if (host.Parent is Border surface)
        {
            surface.CanDrag = false;
        }

        void Restore()
        {
            closed = true;

            if (host.Parent is Border owner)
            {
                owner.CanDrag = true;
            }

            if (index >= 0
                && index < host.Children.Count
                && ReferenceEquals(host.Children[index], editor))
            {
                host.Children[index] = label;
            }
        }

        void Commit()
        {
            if (closed)
            {
                return;
            }

            var name = editor.Text.Trim();

            if (name.Length > 0)
            {
                tab.Name = name;
            }

            label.Text = tab.StripTitle;
            Restore();
            TabRenamed?.Invoke(this, tab);
        }

        editor.LostFocus += (_, _) => Commit();
        editor.KeyDown += (_, args) =>
        {
            if (args.Key == VirtualKey.Enter)
            {
                args.Handled = true;
                Commit();
                return;
            }

            if (args.Key == VirtualKey.Escape)
            {
                args.Handled = true;
                Restore();
            }
        };

        host.Children[index] = editor;
        editor.Focus(FocusState.Programmatic);
        editor.SelectAll();
    }

    private static TextBox NewEditor(TerminalTab tab, TextBlock label) => new()
    {
        Text = tab.Title,
        Width = Math.Max(label.ActualWidth, TerminalTabMetrics.MinEditorWidth),
        MinWidth = 0,
        MinHeight = 0,
        Margin = TerminalTabMetrics.ContentGap,
        Padding = new Thickness(0),
        Background = null,
        BorderThickness = new Thickness(0),
        FontFamily = PanelResources.Font(FontKey),
        FontSize = PanelResources.Size(SizeKey),
        FontWeight = label.FontWeight,
        Foreground = PanelResources.Brush(TextKey),
        VerticalAlignment = VerticalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center,
    };

    private static TextBlock NewLabel(TerminalTab tab, bool active) => new()
    {
        Text = tab.StripTitle,
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
        var menu = new MenuFlyout { Placement = FlyoutPlacementMode.Bottom };

        foreach (var profile in _profiles)
        {
            var item = new MenuFlyoutItem { Text = profile.Name ?? Constants.Shell.DefaultProfileName };
            item.Click += (_, _) => ProfileRequested?.Invoke(this, profile);
            menu.Items.Add(item);
        }

        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(NewMenuItem(
            StringKeys.ExplorerOpen,
            () => ExplorerRequested?.Invoke(this, EventArgs.Empty)));

        menu.ShowAt(AddButton);
    }
}
