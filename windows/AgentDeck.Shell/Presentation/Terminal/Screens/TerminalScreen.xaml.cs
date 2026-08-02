using System.ComponentModel;

using AgentDeck.Shell.Presentation.DesignSystem.TextGrid;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace AgentDeck.Shell.Presentation.Terminal.Screens;

public sealed partial class TerminalScreen : UserControl
{
    private const string StrokeKey = "AdStrokeBrush";
    private const string StrokeBrandKey = "AdStrokeBrandBrush";

    private readonly TerminalViewModel _viewModel;

    public TerminalScreen()
    {
        InitializeComponent();

        _viewModel = new TerminalViewModel(AppServices.Daemon, AppServices.Strings);
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.GridChanged += OnGridChanged;

        GridView.Model = _viewModel.Grid;
        GridView.GridSizeChanged += OnGridSizeChanged;

        CanvasBorder.Background = TerminalCanvasBrush.Build(AppServices.Config.Theme?.Terminal);

        Loaded += OnLoaded;
    }

    public Task InjectAsync(string prompt) => _viewModel.SendAsync($"{prompt}\r", CancellationToken.None);

    public void TakeFocus() => TakeFocus(FocusState.Programmatic);

    private void TakeFocus(FocusState state)
    {
        Focus(state);
        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => Focus(state));
    }

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        TakeFocus();
        await _viewModel.StartAsync(CancellationToken.None);
    }

    private void OnGridChanged(object? sender, EventArgs args) => GridView.Invalidate();

    private async void OnGridSizeChanged(object? sender, TextGridSize size) =>
        await _viewModel.ResizeAsync(size.Columns, size.Rows, CancellationToken.None);

    private void OnCanvasPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        args.Handled = true;
        TakeFocus(FocusState.Pointer);
    }

    private async void OnCanvasPointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        var point = args.GetCurrentPoint(GridView);
        if (point.Properties.MouseWheelDelta == 0)
        {
            return;
        }

        args.Handled = true;
        await _viewModel.WheelAsync(
            Math.Sign(point.Properties.MouseWheelDelta),
            GridView.ColumnAt(point.Position.X),
            GridView.RowAt(point.Position.Y),
            CancellationToken.None);
    }

    private void OnCanvasGotFocus(object sender, RoutedEventArgs args) =>
        CanvasBorder.BorderBrush = PanelResources.Brush(StrokeBrandKey);

    private void OnCanvasLostFocus(object sender, RoutedEventArgs args) =>
        CanvasBorder.BorderBrush = PanelResources.Brush(StrokeKey);

    private async void OnCanvasPreviewKeyDown(object sender, KeyRoutedEventArgs args)
    {
        if (KeyEncoder.Encode(args) is not { } sequence)
        {
            return;
        }

        args.Handled = true;
        await _viewModel.SendAsync(sequence, CancellationToken.None);
    }

    private async void OnCharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs args)
    {
        if (!KeyEncoder.IsPrintable(args.Character))
        {
            return;
        }

        args.Handled = true;
        await _viewModel.SendAsync(args.Character.ToString(), CancellationToken.None);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(TerminalViewModel.Status))
        {
            StatusText.Text = _viewModel.Status;
        }
    }
}
