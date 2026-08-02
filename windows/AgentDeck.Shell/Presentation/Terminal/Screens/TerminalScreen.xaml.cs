using System.ComponentModel;

using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.System;

namespace AgentDeck.Shell.Presentation.Terminal.Screens;

public sealed partial class TerminalScreen : UserControl
{
    private readonly TerminalViewModel _viewModel;

    public TerminalScreen()
    {
        InitializeComponent();

        _viewModel = new TerminalViewModel(AppServices.Daemon, AppServices.Strings);
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        InputBox.PlaceholderText = AppServices.Strings.Get(StringKeys.TerminalInputPlaceholder);
        Loaded += OnLoaded;
    }

    public Task InjectAsync(string prompt) => _viewModel.SendAsync($"{prompt}\r", CancellationToken.None);

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        await _viewModel.StartAsync(CancellationToken.None);
    }

    private async void OnInputKeyDown(object sender, KeyRoutedEventArgs args)
    {
        if (args.Key != VirtualKey.Enter)
        {
            return;
        }

        args.Handled = true;
        var text = InputBox.Text;
        InputBox.Text = string.Empty;
        await _viewModel.SendAsync($"{text}\r", CancellationToken.None);
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
