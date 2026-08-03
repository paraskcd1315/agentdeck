namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public readonly record struct TerminalDropRequest(
    TerminalPane Target,
    TerminalSplitOrientation Orientation,
    bool Before);
