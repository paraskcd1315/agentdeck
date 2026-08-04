using System.Collections.ObjectModel;

using AgentDeck.Shell.Data.Layout;
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

        if (LayoutStore.Load() is { Tabs.Count: > 0 } layout)
        {
            await RestoreAsync(layout, cancellationToken);
            return;
        }

        foreach (var profile in ShellProfiles.AutoStarting(Profiles))
        {
            await OpenAsync(profile, cancellationToken);
        }
    }

    public WorkspaceLayout Capture(double panelWidth) => new()
    {
        PanelWidth = panelWidth,
        Tabs = [.. Tabs.Select(tab => new TabLayout
        {
            ProfileId = tab.Profile.Id,
            Name = tab.Name,
            Root = PaneLayoutMapper.Capture(tab.Root),
        })],
    };

    private async Task RestoreAsync(WorkspaceLayout layout, CancellationToken cancellationToken)
    {
        foreach (var saved in layout.Tabs)
        {
            if (saved.Root is not { } root)
            {
                continue;
            }

            var profile = Profiles.FirstOrDefault(candidate => candidate.Id == saved.ProfileId)
                ?? Profiles[0];

            var panes = new List<TerminalPane>();
            var node = BuildNode(root, profile, panes);

            if (panes.Count == 0)
            {
                continue;
            }

            var tab = TerminalTab.FromLayout(profile, node, panes[0], saved.Name);

            Tabs.Add(tab);
            TabsChanged?.Invoke(this, EventArgs.Empty);
            Active = tab;

            foreach (var pane in panes)
            {
                await pane.ViewModel.StartAsync(cancellationToken);
            }
        }
    }

    private PaneNode BuildNode(PaneLayout layout, ShellProfile profile, List<TerminalPane> panes)
    {
        if (layout.Children is { Count: > 0 } children)
        {
            var split = PaneNode.Split(
                PaneLayoutMapper.Orientation(layout),
                [.. children.Select(child => BuildNode(child, profile, panes))]);

            split.Weight = layout.Weight;
            return split;
        }

        var pane = new TerminalPane(
            layout.Title ?? profile.Name ?? Constants.Shell.DefaultProfileName,
            new TerminalViewModel(_client, _strings, profile));

        panes.Add(pane);

        var leaf = PaneNode.Leaf(pane);
        leaf.Weight = layout.Weight;
        return leaf;
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

        var viewModel = new TerminalViewModel(_client, _strings, tab.Profile);
        var pane = tab.Add(viewModel, orientation);

        PanesChanged?.Invoke(this, EventArgs.Empty);
        await viewModel.StartAsync(cancellationToken);

        return pane;
    }

    public void MovePane(
        TerminalPane pane,
        TerminalPane target,
        TerminalSplitOrientation orientation,
        bool before)
    {
        if (_active is { } tab && tab.Move(pane, target, orientation, before))
        {
            PanesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void MovePaneToNewTab(TerminalPane pane)
    {
        if (_active is not { } tab || tab.Detach(pane) is null)
        {
            return;
        }

        var created = TerminalTab.Adopt(tab.Profile, pane);

        Tabs.Add(created);
        TabsChanged?.Invoke(this, EventArgs.Empty);
        Active = created;
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
