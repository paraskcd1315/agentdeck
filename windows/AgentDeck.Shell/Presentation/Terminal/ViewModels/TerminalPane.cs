namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalPane
{
    public TerminalPane(string title, TerminalViewModel viewModel)
    {
        Title = title;
        ViewModel = viewModel;
    }

    public string Title { get; }

    public TerminalViewModel ViewModel { get; }
}
