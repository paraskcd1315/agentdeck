namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public readonly record struct TerminalSplitRequest(
    TerminalPane Pane,
    TerminalSplitOrientation Orientation);
