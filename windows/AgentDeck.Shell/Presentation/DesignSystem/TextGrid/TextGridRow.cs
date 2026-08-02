namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridRow
{
    public IReadOnlyList<TextGridRun> Runs { get; set; } = Array.Empty<TextGridRun>();
}
