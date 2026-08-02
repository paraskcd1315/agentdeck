namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalPane
{
    public TerminalPane(TerminalViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public TerminalViewModel ViewModel { get; }
}
