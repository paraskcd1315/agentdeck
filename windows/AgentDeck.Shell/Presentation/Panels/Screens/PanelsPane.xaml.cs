using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Screens;

public sealed partial class PanelsPane : UserControl
{
    private readonly PanelsViewModel _viewModel;

    public PanelsPane()
    {
        InitializeComponent();

        _viewModel = new PanelsViewModel(AppServices.Daemon);
        _viewModel.PanelsLoaded += OnPanelsLoaded;
        Loaded += OnLoaded;
    }

    public event EventHandler<PanelButton>? ButtonInvoked;

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        await _viewModel.OpenAsync(Environment.CurrentDirectory, CancellationToken.None);
    }

    private void OnPanelsLoaded(object? sender, IReadOnlyList<PanelDefinition> panels)
    {
        PanelHost.Children.Clear();

        foreach (var panel in panels)
        {
            PanelHost.Children.Add(new PanelView(panel, Invoke));
        }
    }

    private Task Invoke(PanelButton button)
    {
        ButtonInvoked?.Invoke(this, button);
        return Task.CompletedTask;
    }
}
