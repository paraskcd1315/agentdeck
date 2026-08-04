using AgentDeck.Shell.Data.Layout;
using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.DesignSystem.Foundation;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

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
        SetTitleBar(DragRegion);
        AppTitleBar.Loaded += (_, _) => SizeCaptionSpace();
        AppWindow.Changed += (_, _) => SizeCaptionSpace();

        AppWindow.SetIcon("Assets/AppIcon.ico");

        Title = AppServices.Strings.Get(StringKeys.WindowTitle);
        Workspace.Show(ShellResolver.Cwd(AppServices.Config.Shell));

        Terminal.AttachTabStrip(TitleTabs);
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

    private void SaveLayout() => LayoutStore.Save(Terminal.CaptureLayout(PanelColumn.ActualWidth));

    private void OnPaneDragChanged(object? sender, bool dragging)
    {
        var source = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);

        if (!dragging)
        {
            source.ClearRegionRects(NonClientRegionKind.Passthrough);
            return;
        }

        if (Content?.XamlRoot is not { RasterizationScale: > 0 } root)
        {
            return;
        }

        source.SetRegionRects(NonClientRegionKind.Passthrough, [new RectInt32(
            0,
            0,
            (int)(AppTitleBar.ActualWidth * root.RasterizationScale),
            (int)(AppTitleBar.ActualHeight * root.RasterizationScale))]);
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
