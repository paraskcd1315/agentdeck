using System.ComponentModel;

using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

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

        var terminalTheme = AppServices.Config.Theme?.Terminal;
        CanvasBorder.Background = TerminalCanvasBrush.Build(terminalTheme);

        var foreground = ColorParser.Parse(terminalTheme?.Foreground);
        if (foreground is { } color)
        {
            OutputText.Foreground = new SolidColorBrush(color);
        }

        Loaded += OnLoaded;
    }

    public Task InjectAsync(string prompt) => _viewModel.SendAsync($"{prompt}\r", CancellationToken.None);

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        CanvasBorder.Focus(FocusState.Programmatic);
        await _viewModel.StartAsync(CancellationToken.None);
    }

    private void OnCanvasPointerPressed(object sender, PointerRoutedEventArgs args) =>
        CanvasBorder.Focus(FocusState.Pointer);

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
        switch (args.PropertyName)
        {
            case nameof(TerminalViewModel.Output):
                OutputText.Text = _viewModel.Output;
                OutputScroller.ChangeView(null, OutputScroller.ScrollableHeight, null, true);
                break;

            case nameof(TerminalViewModel.Status):
                StatusText.Text = _viewModel.Status;
                break;
        }
    }
}
