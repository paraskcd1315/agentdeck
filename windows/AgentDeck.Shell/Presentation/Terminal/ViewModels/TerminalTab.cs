using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Utils;


namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalTab
{
    private TerminalPane _active;

    public TerminalTab(ShellProfile profile, TerminalViewModel viewModel)
        : this(profile, new TerminalPane(profile.Name ?? Constants.Shell.DefaultProfileName, viewModel))
    {
    }

    private TerminalTab(ShellProfile profile, TerminalPane pane)
    {
        Profile = profile;
        _active = pane;
        Root = PaneNode.Leaf(pane);
    }

    public ShellProfile Profile { get; }

    public PaneNode Root { get; private set; }

    public IReadOnlyList<TerminalPane> Panes => [.. PaneTree.Leaves(Root)];

    public TerminalPane Active
    {
        get => _active;
        set
        {
            if (Panes.Contains(value))
            {
                _active = value;
            }
        }
    }

    public TerminalViewModel ViewModel => _active.ViewModel;

    public string? Name { get; set; }

    public string Title => Name ?? Profile.Name ?? Constants.Shell.DefaultProfileName;

    public string StripTitle => TerminalChrome.TabTitle(Title, Panes.Count);

    public TerminalPane Add(TerminalViewModel viewModel, TerminalSplitOrientation orientation)
    {
        var pane = new TerminalPane(Title, viewModel);

        Root = PaneTree.Insert(Root, _active, pane, orientation);
        _active = pane;

        return pane;
    }

    public static TerminalTab Adopt(ShellProfile profile, TerminalPane pane)
    {
        var tab = new TerminalTab(profile, pane);
        return tab;
    }

    public bool Move(
        TerminalPane pane,
        TerminalPane target,
        TerminalSplitOrientation orientation,
        bool before)
    {
        if (ReferenceEquals(pane, target) || Panes.Count == 1)
        {
            return false;
        }

        var detached = PaneTree.Remove(Root, pane);

        if (detached is null)
        {
            return false;
        }

        Root = PaneTree.Insert(detached, target, pane, orientation, before);
        _active = pane;
        return true;
    }

    public TerminalPane? Detach(TerminalPane pane)
    {
        if (Panes.Count == 1)
        {
            return null;
        }

        var remaining = PaneTree.Remove(Root, pane);

        if (remaining is null)
        {
            return null;
        }

        Root = remaining;
        _active = Panes[0];
        return pane;
    }

    public void Remove(TerminalPane pane)
    {
        if (Panes.Count == 1)
        {
            return;
        }

        Root = PaneTree.Remove(Root, pane) ?? Root;

        if (ReferenceEquals(_active, pane))
        {
            _active = Panes[0];
        }
    }
}
