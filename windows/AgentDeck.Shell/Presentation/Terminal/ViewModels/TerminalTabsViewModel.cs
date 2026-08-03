using System.Collections.ObjectModel;

using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Domain.Interfaces;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalTabsViewModel
{
    private readonly IDaemonClient _client;
    private readonly IStringProvider _strings;

    private TerminalTab? _active;

    public TerminalTabsViewModel(IDaemonClient client, IStringProvider strings)
    {
        _client = client;
        _strings = strings;
        Profiles = ShellProfiles.Resolve(AppServices.Config);
    }

    public event EventHandler? ActiveChanged;

    public event EventHandler? TabsChanged;

    public event EventHandler? PanesChanged;

    public ObservableCollection<TerminalTab> Tabs { get; } = [];

    public IReadOnlyList<ShellProfile> Profiles { get; }

    public TerminalTab? Active
    {
        get => _active;
        private set
        {
            if (ReferenceEquals(_active, value))
            {
                return;
            }

            _active = value;
            ActiveChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Tabs.Count > 0)
        {
            return;
        }

        foreach (var profile in ShellProfiles.AutoStarting(Profiles))
        {
            await OpenAsync(profile, cancellationToken);
        }
    }

    public async Task<TerminalTab> OpenAsync(ShellProfile profile, CancellationToken cancellationToken)
    {
        var tab = new TerminalTab(profile, new TerminalViewModel(_client, _strings, profile));

        Tabs.Add(tab);
        TabsChanged?.Invoke(this, EventArgs.Empty);
        Active = tab;

        await tab.ViewModel.StartAsync(cancellationToken);
        return tab;
    }

    public async Task<TerminalPane?> SplitAsync(
        TerminalSplitOrientation orientation,
        CancellationToken cancellationToken)
    {
        if (_active is not { } tab)
        {
            return null;
        }

        tab.Orientation = orientation;

        var viewModel = new TerminalViewModel(_client, _strings, tab.Profile);
        var pane = tab.Add(viewModel);

        PanesChanged?.Invoke(this, EventArgs.Empty);
        await viewModel.StartAsync(cancellationToken);

        return pane;
    }

    public async Task ClosePaneAsync(TerminalPane pane, CancellationToken cancellationToken)
    {
        if (_active is not { } tab || tab.Panes.Count == 1)
        {
            return;
        }

        tab.Remove(pane);
        PanesChanged?.Invoke(this, EventArgs.Empty);
        await pane.ViewModel.CloseAsync(cancellationToken);
    }

    public async Task CloseAsync(TerminalTab tab, CancellationToken cancellationToken)
    {
        var index = Tabs.IndexOf(tab);
        if (index < 0)
        {
            return;
        }

        Tabs.RemoveAt(index);
        TabsChanged?.Invoke(this, EventArgs.Empty);

        if (ReferenceEquals(Active, tab))
        {
            Active = Tabs.Count == 0 ? null : Tabs[Math.Min(index, Tabs.Count - 1)];
        }

        foreach (var pane in tab.Panes)
        {
            await pane.ViewModel.CloseAsync(cancellationToken);
        }
    }

    public void Activate(TerminalTab tab)
    {
        if (Tabs.Contains(tab))
        {
            Active = tab;
        }
    }
}
