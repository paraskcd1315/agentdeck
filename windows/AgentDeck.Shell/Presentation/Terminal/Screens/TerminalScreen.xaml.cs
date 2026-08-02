using System.ComponentModel;

using AgentDeck.Shell.Domain.Entities;
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
    private const string StrokeKey = "AdStrokeBrush";
    private const string StrokeBrandKey = "AdStrokeBrandBrush";

    private readonly TerminalTabsViewModel _tabs;

    private TerminalViewModel? _bound;

    public TerminalScreen()
    {
        InitializeComponent();

        _tabs = new TerminalTabsViewModel(AppServices.Daemon, AppServices.Strings);
        _tabs.ActiveChanged += OnActiveChanged;
        _tabs.TabsChanged += OnTabsChanged;

        TabStrip.TabSelected += OnTabSelected;
        TabStrip.TabClosed += OnTabClosed;
        TabStrip.ProfileRequested += OnProfileRequested;

        GridView.GridSizeChanged += OnGridSizeChanged;

        CanvasBorder.Background = TerminalCanvasBrush.Build(AppServices.Config.Theme?.Terminal);

        Loaded += OnLoaded;
    }

    private TerminalViewModel? Active => _tabs.Active?.ViewModel;

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

        if (_bound is { } previous)
        {
            previous.PropertyChanged -= OnViewModelPropertyChanged;
            previous.GridChanged -= OnGridChanged;
        }

        _bound = _tabs.Active?.ViewModel;

        if (_bound is not { } viewModel)
        {
            return;
        }

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        viewModel.GridChanged += OnGridChanged;

        GridView.Model = viewModel.Grid;
        StatusText.Text = viewModel.Status;
        OnGridChanged(this, EventArgs.Empty);
        TakeFocus(FocusState.Programmatic);
    }

    private void RenderTabs() => TabStrip.Render([.. _tabs.Tabs], _tabs.Active, _tabs.Profiles);

    private void OnTabSelected(object? sender, TerminalTab tab) => _tabs.Activate(tab);

    private async void OnTabClosed(object? sender, TerminalTab tab) =>
        await _tabs.CloseAsync(tab, CancellationToken.None);

    private async void OnProfileRequested(object? sender, ShellProfile profile) =>
        await _tabs.OpenAsync(profile, CancellationToken.None);

    private void OnGridChanged(object? sender, EventArgs args)
    {
        if (Active is not { } viewModel)
        {
            return;
        }

        GridView.Invalidate();
        ScrollIndicator.Update(viewModel.History, viewModel.DisplayOffset, viewModel.Grid.Rows);
    }

    private async void OnGridSizeChanged(object? sender, TextGridSize size)
    {
        foreach (var tab in _tabs.Tabs.ToList())
        {
            await tab.ViewModel.ResizeAsync(size.Columns, size.Rows, CancellationToken.None);
        }
    }

    private async void OnCanvasPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        args.Handled = true;
        TakeFocus(FocusState.Pointer);

        var point = args.GetCurrentPoint(GridView);
        await SendButtonAsync(point, PointerButtons.Pressed(point.Properties), true);
    }

    private async void OnCanvasPointerReleased(object sender, PointerRoutedEventArgs args)
    {
        args.Handled = true;

        var point = args.GetCurrentPoint(GridView);
        await SendButtonAsync(point, PointerButtons.Released(point.Properties.PointerUpdateKind), false);
    }

    private Task SendButtonAsync(PointerPoint point, TerminalMouseButton? button, bool pressed)
    {
        if (button is not { } value || Active is not { } viewModel)
        {
            return Task.CompletedTask;
        }

        return viewModel.ButtonAsync(
            value,
            pressed,
            GridView.ColumnAt(point.Position.X),
            GridView.RowAt(point.Position.Y),
            CancellationToken.None);
    }

    private async void OnCanvasPointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        var point = args.GetCurrentPoint(GridView);
        if (point.Properties.MouseWheelDelta == 0 || Active is not { } viewModel)
        {
            return;
        }

        args.Handled = true;
        await viewModel.WheelAsync(
            Math.Sign(point.Properties.MouseWheelDelta),
            GridView.ColumnAt(point.Position.X),
            GridView.RowAt(point.Position.Y),
            CancellationToken.None);
    }

    private async void OnCanvasGotFocus(object sender, RoutedEventArgs args)
    {
        CanvasBorder.BorderBrush = PanelResources.Brush(StrokeBrandKey);

        if (Active is { } viewModel)
        {
            await viewModel.RefreshAsync(CancellationToken.None);
        }
    }

    private void OnCanvasLostFocus(object sender, RoutedEventArgs args) =>
        CanvasBorder.BorderBrush = PanelResources.Brush(StrokeKey);

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

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(TerminalViewModel.Status) && sender is TerminalViewModel viewModel)
        {
            StatusText.Text = viewModel.Status;
        }
    }
}
