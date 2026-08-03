using System.Collections.ObjectModel;

using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalTab
{
    private TerminalPane _active;

    public TerminalTab(ShellProfile profile, TerminalViewModel viewModel)
    {
        Profile = profile;
        _active = new TerminalPane(profile.Name ?? Constants.Shell.DefaultProfileName, viewModel);
        Panes.Add(_active);
    }

    public ShellProfile Profile { get; }

    public ObservableCollection<TerminalPane> Panes { get; } = [];

    public TerminalSplitOrientation Orientation { get; set; } = TerminalSplitOrientation.Horizontal;

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

    public TerminalPane Add(TerminalViewModel viewModel)
    {
        var pane = new TerminalPane(Title, viewModel);
        Panes.Add(pane);
        _active = pane;
        return pane;
    }

    public void Remove(TerminalPane pane)
    {
        var index = Panes.IndexOf(pane);
        if (index < 0 || Panes.Count == 1)
        {
            return;
        }

        Panes.RemoveAt(index);

        if (ReferenceEquals(_active, pane))
        {
            _active = Panes[Math.Min(index, Panes.Count - 1)];
        }
    }
}
