using System.ComponentModel;

using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.DesignSystem.Foundation;
using AgentDeck.Shell.Presentation.DesignSystem.TextGrid;
using AgentDeck.Shell.Presentation.Terminal.Components;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace AgentDeck.Shell.Presentation.Terminal.Screens;

public sealed partial class TerminalScreen : UserControl
{
    private const double FocusedOpacity = 1;
    private const double UnfocusedOpacity = 0.55;
    private const string StrokeKey = "AdStrokeBrush";

    private readonly TerminalTabsViewModel _tabs;

    private readonly List<TerminalPaneView> _paneViews = [];

    private TerminalTabStrip? _strip;
    private TerminalPane? _dragging;

    public TerminalScreen()
    {
        InitializeComponent();

        _tabs = new TerminalTabsViewModel(AppServices.Daemon, AppServices.Strings);
        _tabs.ActiveChanged += OnActiveChanged;
        _tabs.TabsChanged += OnTabsChanged;
        _tabs.PanesChanged += OnPanesChanged;

        CanvasHost.Background = TerminalCanvasBrush.Build(AppServices.Config.Theme?.Terminal);

        Loaded += OnLoaded;
    }

    private TerminalViewModel? Active => _tabs.Active?.Active.ViewModel;

    public event EventHandler? LayoutChanged;

    public event EventHandler<bool>? PaneDragChanged;

    public WorkspaceLayout CaptureLayout(double panelWidth) => _tabs.Capture(panelWidth);

    public bool HasPaneInFlight => _dragging is not null;

    public void DropPaneAsNewTab() => OnPaneDroppedOnStrip(this, EventArgs.Empty);

    public void AttachTabStrip(TerminalTabStrip strip)
    {
        _strip = strip;
        strip.TabSelected += OnTabSelected;
        strip.TabClosed += OnTabClosed;
        strip.ProfileRequested += OnProfileRequested;
        strip.PaneDroppedOnStrip += OnPaneDroppedOnStrip;
        RenderTabs();
    }

    public Task InjectAsync(string prompt) =>
        Active?.SendAsync($"{prompt}\r", CancellationToken.None) ?? Task.CompletedTask;

    public void TakeFocus() => TakeFocus(FocusState.Programmatic);

    private void TakeFocus(FocusState state)
    {
        Focus(state);
        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => Focus(state));
    }

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        TakeFocus();
        await _tabs.StartAsync(CancellationToken.None);
    }

    private void OnTabsChanged(object? sender, EventArgs args)
    {
        RenderTabs();
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnActiveChanged(object? sender, EventArgs args)
    {
        RenderTabs();
        RenderPanes();
        TakeFocus(FocusState.Programmatic);
    }

    private void OnPanesChanged(object? sender, EventArgs args)
    {
        RenderPanes();
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RenderPanes()
    {
        CanvasHost.Children.Clear();
        _paneViews.Clear();

        if (_tabs.Active is not { } tab)
        {
            return;
        }

        CanvasHost.Children.Add(BuildNode(tab.Root, tab));
    }

    private FrameworkElement BuildNode(PaneNode node, TerminalTab tab)
    {
        if (node.Pane is { } pane)
        {
            var panes = tab.Panes.ToList();
            return BuildPaneView(pane, tab, panes.IndexOf(pane) + 1, panes.Count);
        }

        var vertical = node.Orientation == TerminalSplitOrientation.Vertical;
        var grid = new Grid();

        for (var index = 0; index < node.Children.Count; index++)
        {
            var weight = new GridLength(node.Children[index].Weight, GridUnitType.Star);

            if (vertical)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = weight });
            }
            else
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = weight });
            }

            var child = BuildNode(node.Children[index], tab);
            Place(child, index, vertical);
            grid.Children.Add(child);

            if (index > 0)
            {
                var divider = BuildPaneDivider(grid, node, index, vertical);
                Place(divider, index, vertical);
                grid.Children.Add(divider);
            }
        }

        return grid;
    }

    private static void Place(FrameworkElement element, int index, bool vertical)
    {
        if (vertical)
        {
            Grid.SetRow(element, index);
            return;
        }

        Grid.SetColumn(element, index);
    }

    private TerminalPaneView BuildPaneView(TerminalPane pane, TerminalTab tab, int index, int total)
    {
        var view = new TerminalPaneView();
        view.Bind(pane, ReferenceEquals(tab.Active, pane), total > 1, index, total);
        view.GridSizeChanged += OnGridSizeChanged;
        view.SplitRequested += OnPaneSplitRequested;
        view.CloseRequested += OnPaneCloseRequested;
        view.DragStarted += OnPaneDragStarted;
        view.DragEnded += OnPaneDragEnded;
        view.PaneDropped += OnPaneDropped;
        _paneViews.Add(view);
        return view;
    }

    private async void OnPaneSplitRequested(object? sender, TerminalSplitRequest request)
    {
        if (_tabs.Active is { } tab)
        {
            tab.Active = request.Pane;
        }

        await _tabs.SplitAsync(request.Orientation, CancellationToken.None);
        TakeFocus(FocusState.Programmatic);
    }

    private void OnPaneDragStarted(object? sender, TerminalPane pane)
    {
        _dragging = pane;
        PaneDragChanged?.Invoke(this, true);
    }

    private void OnPaneDragEnded(object? sender, EventArgs args)
    {
        _dragging = null;
        PaneDragChanged?.Invoke(this, false);
    }

    private void OnPaneDropped(object? sender, TerminalDropRequest request)
    {
        if (_dragging is not { } pane)
        {
            return;
        }

        _dragging = null;
        _tabs.MovePane(pane, request.Target, request.Orientation, request.Before);
        TakeFocus(FocusState.Programmatic);
    }

    private void OnPaneDroppedOnStrip(object? sender, EventArgs args)
    {
        if (_dragging is not { } pane)
        {
            return;
        }

        _dragging = null;
        _tabs.MovePaneToNewTab(pane);
        TakeFocus(FocusState.Programmatic);
    }

    private async void OnPaneCloseRequested(object? sender, TerminalPane pane)
    {
        await _tabs.ClosePaneAsync(pane, CancellationToken.None);
        TakeFocus(FocusState.Programmatic);
    }

    private DragDivider BuildPaneDivider(Grid host, PaneNode node, int index, bool vertical)
    {
        var divider = new DragDivider
        {
            HorizontalAlignment = vertical ? HorizontalAlignment.Stretch : HorizontalAlignment.Left,
            VerticalAlignment = vertical ? VerticalAlignment.Top : VerticalAlignment.Stretch,
        };

        divider.SetVertical(vertical);
        divider.Dragged += (_, delta) => ResizePanes(host, node, index, delta, vertical);
        return divider;
    }

    private void ResizePanes(Grid host, PaneNode node, int index, double delta, bool vertical)
    {
        var minimum = vertical ? TerminalMetrics.MinimumPaneHeight : TerminalMetrics.MinimumPaneWidth;

        var firstSize = (vertical
            ? host.RowDefinitions[index - 1].ActualHeight
            : host.ColumnDefinitions[index - 1].ActualWidth) + delta;

        var secondSize = (vertical
            ? host.RowDefinitions[index].ActualHeight
            : host.ColumnDefinitions[index].ActualWidth) - delta;

        if (firstSize < minimum || secondSize < minimum)
        {
            return;
        }

        node.Children[index - 1].Weight = firstSize;
        node.Children[index].Weight = secondSize;

        if (vertical)
        {
            host.RowDefinitions[index - 1].Height = new GridLength(firstSize, GridUnitType.Star);
            host.RowDefinitions[index].Height = new GridLength(secondSize, GridUnitType.Star);
        }
        else
        {
            host.ColumnDefinitions[index - 1].Width = new GridLength(firstSize, GridUnitType.Star);
            host.ColumnDefinitions[index].Width = new GridLength(secondSize, GridUnitType.Star);
        }

        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RenderTabs() => _strip?.Render([.. _tabs.Tabs], _tabs.Active, _tabs.Profiles);

    private void OnTabSelected(object? sender, TerminalTab tab) => _tabs.Activate(tab);

    private async void OnTabClosed(object? sender, TerminalTab tab) =>
        await _tabs.CloseAsync(tab, CancellationToken.None);

    private async void OnProfileRequested(object? sender, ShellProfile profile) =>
        await _tabs.OpenAsync(profile, CancellationToken.None);

    private async void OnGridSizeChanged(object? sender, TextGridSize size)
    {
        if (sender is not TerminalPaneView { Pane: { } pane })
        {
            return;
        }

        await pane.ViewModel.ResizeAsync(size.Columns, size.Rows, CancellationToken.None);
    }

    private TerminalPaneView? PaneAt(PointerRoutedEventArgs args)
    {
        foreach (var view in _paneViews)
        {
            var local = args.GetCurrentPoint(view).Position;

            if (local.X >= 0 && local.Y >= 0 && local.X < view.ActualWidth && local.Y < view.ActualHeight)
            {
                return view;
            }
        }

        return null;
    }

    private async void OnCanvasPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        args.Handled = true;
        TakeFocus(FocusState.Pointer);

        if (PaneAt(args) is not { Pane: { } pane } view)
        {
            return;
        }

        if (_tabs.Active is { } tab && !ReferenceEquals(tab.Active, pane))
        {
            tab.Active = pane;
            SetPaneHighlights();
        }

        var point = args.GetCurrentPoint(view);
        await SendButtonAsync(view, point, PointerButtons.Pressed(point.Properties), true);
    }

    private async void OnCanvasPointerReleased(object sender, PointerRoutedEventArgs args)
    {
        args.Handled = true;

        if (PaneAt(args) is not { } view)
        {
            return;
        }

        var point = args.GetCurrentPoint(view);
        await SendButtonAsync(view, point, PointerButtons.Released(point.Properties.PointerUpdateKind), false);
    }

    private Task SendButtonAsync(
        TerminalPaneView view,
        PointerPoint point,
        TerminalMouseButton? button,
        bool pressed)
    {
        if (button is not { } value || view.Pane?.ViewModel is not { } viewModel)
        {
            return Task.CompletedTask;
        }

        return viewModel.ButtonAsync(
            value,
            pressed,
            view.ColumnAt(point.Position.X),
            view.RowAt(point.Position.Y),
            CancellationToken.None);
    }

    private async void OnCanvasPointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        if (PaneAt(args) is not { Pane: { } pane } view)
        {
            return;
        }

        var point = args.GetCurrentPoint(view);
        if (point.Properties.MouseWheelDelta == 0)
        {
            return;
        }

        args.Handled = true;
        await pane.ViewModel.WheelAsync(
            Math.Sign(point.Properties.MouseWheelDelta),
            view.ColumnAt(point.Position.X),
            view.RowAt(point.Position.Y),
            CancellationToken.None);
    }

    private async void OnCanvasGotFocus(object sender, RoutedEventArgs args)
    {
        SetPaneHighlights();

        if (Active is { } viewModel)
        {
            await viewModel.RefreshAsync(CancellationToken.None);
        }
    }

    private void OnCanvasLostFocus(object sender, RoutedEventArgs args) => SetPaneHighlights();

    private void SetPaneHighlights()
    {
        foreach (var view in _paneViews)
        {
            if (view.Pane is { } pane)
            {
                view.SetActive(ReferenceEquals(_tabs.Active?.Active, pane));
            }
        }
    }

    private async void OnCanvasPreviewKeyDown(object sender, KeyRoutedEventArgs args)
    {
        if (Active is not { } viewModel)
        {
            return;
        }

        if (!viewModel.UsesAlternateScreen && ScrollKeys.Pages(args) is { } pages)
        {
            args.Handled = true;
            await viewModel.ScrollPageAsync(pages, CancellationToken.None);
            return;
        }

        if (KeyEncoder.Encode(args) is not { } sequence)
        {
            return;
        }

        args.Handled = true;
        await viewModel.SendAsync(sequence, CancellationToken.None);
    }

    private async void OnCharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs args)
    {
        if (!KeyEncoder.IsPrintable(args.Character) || Active is not { } viewModel)
        {
            return;
        }

        args.Handled = true;
        await viewModel.SendAsync(args.Character.ToString(), CancellationToken.None);
    }

}
