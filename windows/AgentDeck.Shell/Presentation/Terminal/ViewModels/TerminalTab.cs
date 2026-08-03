using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalTab
{
    private TerminalPane _active;

    public TerminalTab(ShellProfile profile, TerminalViewModel viewModel)
    {
        Profile = profile;
        _active = new TerminalPane(profile.Name ?? Constants.Shell.DefaultProfileName, viewModel);
        Root = PaneNode.Leaf(_active);
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

    public string Title => Profile.Name ?? Constants.Shell.DefaultProfileName;

    public TerminalPane Add(TerminalViewModel viewModel, TerminalSplitOrientation orientation)
    {
        var pane = new TerminalPane(Title, viewModel);

        Root = PaneTree.Insert(Root, _active, pane, orientation);
        _active = pane;

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
