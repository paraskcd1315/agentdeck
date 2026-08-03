namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class TerminalGlyphs
{
    private const int SplitCodepoint = 0xE7C4;
    private const int CloseCodepoint = 0xE711;

    public static string Split { get; } = char.ConvertFromUtf32(SplitCodepoint);

    public static string Close { get; } = char.ConvertFromUtf32(CloseCodepoint);
}
