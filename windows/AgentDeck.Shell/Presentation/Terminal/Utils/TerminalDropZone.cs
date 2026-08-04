using AgentDeck.Shell.Presentation.Terminal.ViewModels;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public readonly record struct TerminalDropZone(TerminalSplitOrientation Orientation, bool Before);
