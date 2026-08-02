using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalTab
{
    public TerminalTab(ShellProfile profile, TerminalViewModel viewModel)
    {
        Profile = profile;
        ViewModel = viewModel;
    }

    public ShellProfile Profile { get; }

    public TerminalViewModel ViewModel { get; }

    public string Title => Profile.Name ?? Constants.Shell.DefaultProfileName;
}
