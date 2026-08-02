namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridSize
{
    public TextGridSize(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;
    }

    public int Columns { get; }

    public int Rows { get; }
}
