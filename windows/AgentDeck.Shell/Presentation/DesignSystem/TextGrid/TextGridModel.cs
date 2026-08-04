namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridModel
{
    private TextGridRow[] _rows = [];

    public int Columns { get; private set; }

    public int Rows => _rows.Length;

    public TextGridCursor Cursor { get; } = new();

    public IReadOnlyList<TextGridRow> Lines => _rows;

    public void Resize(int columns, int rows)
    {
        Columns = columns;

        if (_rows.Length == rows)
        {
            return;
        }

        var replacement = new TextGridRow[rows];
        for (var index = 0; index < rows; index++)
        {
            replacement[index] = index < _rows.Length ? _rows[index] : new TextGridRow();
        }

        _rows = replacement;
    }

    public void Clear()
    {
        foreach (var row in _rows)
        {
            row.Runs = Array.Empty<TextGridRun>();
        }
    }

    public void SetRow(int index, IReadOnlyList<TextGridRun> runs)
    {
        if (index < 0 || index >= _rows.Length)
        {
            return;
        }

        _rows[index].Runs = runs;
    }
}
