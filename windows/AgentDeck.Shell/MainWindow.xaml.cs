using AgentDeck.Shell.Data.Layout;
using AgentDeck.Shell.Data.Windows;
using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.DesignSystem.Foundation;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;

namespace AgentDeck.Shell;

public sealed partial class MainWindow : Window
{
    private readonly bool _primary;

    private bool _paneDragging;

    public MainWindow()
        : this(null)
    {
    }

    private MainWindow(TerminalTab? adopted)
    {
        _primary = adopted is null;

        InitializeComponent();

        SystemBackdrop = new AdAcrylicBackdrop { Kind = DesktopAcrylicKind.Thin };

        var theme = AppServices.Config.Theme;
        AppGradient.Fill = AppBackgroundBrush.Build(theme);
        AppGradient.Opacity = AppBackgroundBrush.Opacity(theme);

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(DragRegion);
        AppTitleBar.Loaded += (_, _) => SizeCaptionSpace();
        AppWindow.Changed += (_, _) => SizeCaptionSpace();

        AppWindow.SetIcon("Assets/AppIcon.ico");

        Title = AppServices.Strings.Get(StringKeys.WindowTitle);
        Workspace.Show(ShellResolver.Cwd(AppServices.Config.Shell));

        if (adopted is { } tab)
        {
            Terminal.AdoptTab(tab);
        }

        Terminal.AttachTabStrip(TitleTabs);
        TitleTabs.RegionsChanged += (_, _) => UpdatePassthrough();
        Terminal.TabTornOff += OnTabTornOff;
        PanelDivider.Dragged += OnPanelDividerDragged;

        Panels.ButtonInvoked += OnPanelButtonInvoked;
        Terminal.LayoutChanged += (_, _) => SaveLayout();
        Terminal.PaneDragChanged += OnPaneDragChanged;
        Activated += OnActivated;
        Closed += (_, _) => SaveLayout();

        if (LayoutStore.Load() is { PanelWidth: > 0 } layout)
        {
            PanelColumn.Width = new GridLength(layout.PanelWidth);
        }
    }

    private void OnPanelDividerDragged(object? sender, double delta)
    {
        var width = PanelColumn.ActualWidth - delta;
        var terminal = Terminal.ActualWidth + delta;

        if (width < TerminalMetrics.MinimumPanelWidth || terminal < TerminalMetrics.MinimumTerminalWidth)
        {
            return;
        }

        PanelColumn.Width = new GridLength(width);
        SaveLayout();
    }

    private void SaveLayout()
    {
        if (_primary)
        {
            LayoutStore.Save(Terminal.CaptureLayout(PanelColumn.ActualWidth));
        }
    }

    private void OnTabTornOff(object? sender, TerminalTab tab)
    {
        if (WindowBounds.Contains(WindowRect(), CursorPosition.Current()))
        {
            return;
        }

        if (Terminal.ReleaseTab(tab) is not { } released)
        {
            return;
        }

        new MainWindow(released).Activate();
    }

    private RectInt32 WindowRect() => new(
        AppWindow.Position.X,
        AppWindow.Position.Y,
        AppWindow.Size.Width,
        AppWindow.Size.Height);

    private void OnPaneDragChanged(object? sender, bool dragging)
    {
        _paneDragging = dragging;
        UpdatePassthrough();
    }

    private void UpdatePassthrough()
    {
        if (Content?.XamlRoot is not { RasterizationScale: > 0 } root)
        {
            return;
        }

        IReadOnlyList<Rect> regions = _paneDragging
            ? [new Rect(0, 0, AppTitleBar.ActualWidth, AppTitleBar.ActualHeight)]
            : TitleTabs.InteractiveRegions(Content);

        InputNonClientPointerSource
            .GetForWindowId(AppWindow.Id)
            .SetRegionRects(
                NonClientRegionKind.Passthrough,
                PassthroughRegions.Scale(regions, root.RasterizationScale));
    }

    private void OnTitleBarDragOver(object sender, DragEventArgs args)
    {
        if (!Terminal.HasPaneInFlight)
        {
            return;
        }

        args.AcceptedOperation = DataPackageOperation.Move;
        args.DragUIOverride.IsGlyphVisible = false;
        args.Handled = true;
    }

    private void OnTitleBarDrop(object sender, DragEventArgs args)
    {
        if (!Terminal.HasPaneInFlight)
        {
            return;
        }

        args.Handled = true;
        Terminal.DropPaneAsNewTab();
    }

    private void SizeCaptionSpace()
    {
        if (Content?.XamlRoot is not { RasterizationScale: > 0 } root)
        {
            return;
        }

        CaptionSpace.Width = AppWindow.TitleBar.RightInset / root.RasterizationScale;
        UpdatePassthrough();
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
