using Windows.UI;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridStyle
{
    public required Color Foreground { get; init; }

    public required Color Background { get; init; }

    public bool Bold { get; init; }

    public bool Italic { get; init; }

    public bool Underline { get; init; }

    public bool Strikeout { get; init; }
}
