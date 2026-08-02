namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridRun
{
    public required int Column { get; init; }

    public required string Text { get; init; }

    public required TextGridStyle Style { get; init; }
}
