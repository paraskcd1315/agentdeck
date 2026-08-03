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
        CanvasHost.RowDefinitions.Clear();

        if (_tabs.Active is not { } tab)
        {
            return;
        }

        var vertical = tab.Orientation == TerminalSplitOrientation.Vertical;

        for (var index = 0; index < tab.Panes.Count; index++)
        {
            if (vertical)
            {
                CanvasHost.RowDefinitions.Add(new RowDefinition());
            }
            else
            {
                CanvasHost.ColumnDefinitions.Add(new ColumnDefinition());
            }

            var view = BuildPaneView(tab.Panes[index], index, tab);
            Place(view, index, vertical);
            CanvasHost.Children.Add(view);

            if (index > 0)
            {
                var divider = BuildPaneDivider(index, vertical);
                Place(divider, index, vertical);
                CanvasHost.Children.Add(divider);
            }
        }
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

    private TerminalPaneView BuildPaneView(TerminalPane pane, int index, TerminalTab tab)
    {
        var view = new TerminalPaneView();
        view.Bind(pane, ReferenceEquals(tab.Active, pane), tab.Panes.Count > 1);
        view.GridSizeChanged += OnGridSizeChanged;
        view.SplitRequested += OnPaneSplitRequested;
        view.CloseRequested += OnPaneCloseRequested;
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

    private async void OnPaneCloseRequested(object? sender, TerminalPane pane)
    {
        await _tabs.ClosePaneAsync(pane, CancellationToken.None);
        TakeFocus(FocusState.Programmatic);
    }

    private DragDivider BuildPaneDivider(int index, bool vertical)
    {
        var divider = new DragDivider
        {
            HorizontalAlignment = vertical ? HorizontalAlignment.Stretch : HorizontalAlignment.Left,
            VerticalAlignment = vertical ? VerticalAlignment.Top : VerticalAlignment.Stretch,
        };

        divider.SetVertical(vertical);
        divider.Dragged += (_, delta) => ResizePanes(index, delta, vertical);
        return divider;
    }

    private void ResizePanes(int index, double delta, bool vertical)
    {
        if (vertical)
        {
            ResizeRows(index, delta);
            return;
        }

        if (index <= 0 || index >= CanvasHost.ColumnDefinitions.Count)
        {
            return;
        }

        var first = CanvasHost.ColumnDefinitions[index - 1];
        var second = CanvasHost.ColumnDefinitions[index];
        var firstSize = first.ActualWidth + delta;
        var secondSize = second.ActualWidth - delta;

        if (firstSize < TerminalMetrics.MinimumPaneWidth || secondSize < TerminalMetrics.MinimumPaneWidth)
        {
            return;
        }

        first.Width = new GridLength(firstSize, GridUnitType.Star);
        second.Width = new GridLength(secondSize, GridUnitType.Star);
    }

    private void ResizeRows(int index, double delta)
    {
        if (index <= 0 || index >= CanvasHost.RowDefinitions.Count)
        {
            return;
        }

        var first = CanvasHost.RowDefinitions[index - 1];
        var second = CanvasHost.RowDefinitions[index];
        var firstSize = first.ActualHeight + delta;
        var secondSize = second.ActualHeight - delta;

        if (firstSize < TerminalMetrics.MinimumPaneHeight || secondSize < TerminalMetrics.MinimumPaneHeight)
        {
            return;
        }

        first.Height = new GridLength(firstSize, GridUnitType.Star);
        second.Height = new GridLength(secondSize, GridUnitType.Star);
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
