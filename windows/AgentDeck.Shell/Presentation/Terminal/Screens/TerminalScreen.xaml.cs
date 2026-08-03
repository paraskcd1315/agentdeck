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

    private TerminalTabStrip? _strip;

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

    public void AttachTabStrip(TerminalTabStrip strip)
    {
        _strip = strip;
        strip.TabSelected += OnTabSelected;
        strip.TabClosed += OnTabClosed;
        strip.ProfileRequested += OnProfileRequested;
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

    private void OnTabsChanged(object? sender, EventArgs args) => RenderTabs();

    private void OnActiveChanged(object? sender, EventArgs args)
    {
        RenderTabs();
        RenderPanes();
        TakeFocus(FocusState.Programmatic);
    }

    private void OnPanesChanged(object? sender, EventArgs args) => RenderPanes();

    private void RenderPanes()
    {
        CanvasHost.Children.Clear();
        CanvasHost.ColumnDefinitions.Clear();

        if (_tabs.Active is not { } tab)
        {
            return;
        }

        for (var index = 0; index < tab.Panes.Count; index++)
        {
            CanvasHost.ColumnDefinitions.Add(new ColumnDefinition());
            CanvasHost.Children.Add(BuildPaneView(tab.Panes[index], index, tab));

            if (index > 0)
            {
                CanvasHost.Children.Add(BuildPaneDivider(index));
            }
        }
    }

    private TerminalPaneView BuildPaneView(TerminalPane pane, int column, TerminalTab tab)
    {
        var view = new TerminalPaneView();
        view.Bind(pane, ReferenceEquals(tab.Active, pane), tab.Panes.Count > 1);
        view.GridSizeChanged += OnGridSizeChanged;
        view.SplitRequested += OnPaneSplitRequested;
        view.CloseRequested += OnPaneCloseRequested;
        Grid.SetColumn(view, column);
        return view;
    }

    private async void OnPaneSplitRequested(object? sender, TerminalPane pane)
    {
        if (_tabs.Active is { } tab)
        {
            tab.Active = pane;
        }

        await _tabs.SplitAsync(CancellationToken.None);
        TakeFocus(FocusState.Programmatic);
    }

    private async void OnPaneCloseRequested(object? sender, TerminalPane pane)
    {
        await _tabs.ClosePaneAsync(pane, CancellationToken.None);
        TakeFocus(FocusState.Programmatic);
    }

    private DragDivider BuildPaneDivider(int column)
    {
        var divider = new DragDivider { HorizontalAlignment = HorizontalAlignment.Left };
        divider.Dragged += (_, delta) => ResizePanes(column, delta);

        Grid.SetColumn(divider, column);
        return divider;
    }

    private void ResizePanes(int column, double delta)
    {
        if (column <= 0 || column >= CanvasHost.ColumnDefinitions.Count)
        {
            return;
        }

        var left = CanvasHost.ColumnDefinitions[column - 1];
        var right = CanvasHost.ColumnDefinitions[column];

        var leftWidth = left.ActualWidth + delta;
        var rightWidth = right.ActualWidth - delta;

        if (leftWidth < TerminalMetrics.MinimumPaneWidth || rightWidth < TerminalMetrics.MinimumPaneWidth)
        {
            return;
        }

        left.Width = new GridLength(leftWidth, GridUnitType.Star);
        right.Width = new GridLength(rightWidth, GridUnitType.Star);
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
        foreach (var child in CanvasHost.Children)
        {
            if (child is not TerminalPaneView view)
            {
                continue;
            }

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
        foreach (var child in CanvasHost.Children)
        {
            if (child is TerminalPaneView view && view.Pane is { } pane)
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
