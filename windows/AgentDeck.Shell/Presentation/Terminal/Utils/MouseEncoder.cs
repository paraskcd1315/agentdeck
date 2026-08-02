using AgentDeck.Shell.Data.Daemon.Dto;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class MouseEncoder
{
    private const string Escape = "";
    private const int WheelUpButton = 64;
    private const int WheelDownButton = 65;
    private const int CoordinateOffset = 32;
    private const int MaxLegacyCoordinate = 223;

    public static string? Wheel(GridModeDto? mode, int notches, int columnIndex, int rowIndex)
    {
        if (mode is not { AltScreen: true } || notches == 0)
        {
            return null;
        }

        var up = notches > 0;
        var repeats = Math.Abs(notches);

        if (!mode.MouseReport)
        {
            return mode.AlternateScroll
                ? Repeat(Arrow(mode.ApplicationCursor, up), repeats * TerminalMetrics.WheelScrollLines)
                : null;
        }

        var button = up ? WheelUpButton : WheelDownButton;
        var column = columnIndex + 1;
        var row = rowIndex + 1;
        var report = mode.SgrMouse ? Sgr(button, column, row) : Legacy(button, column, row);

        return Repeat(report, repeats);
    }

    private static string Arrow(bool applicationCursor, bool up)
    {
        var introducer = applicationCursor ? "O" : "[";
        var final = up ? "A" : "B";
        return $"{Escape}{introducer}{final}";
    }

    private static string Sgr(int button, int column, int row) =>
        $"{Escape}[<{button};{column};{row}M";

    private static string Legacy(int button, int column, int row) =>
        $"{Escape}[M{(char)(CoordinateOffset + button)}{Coordinate(column)}{Coordinate(row)}";

    private static char Coordinate(int value) =>
        (char)(CoordinateOffset + Math.Clamp(value, 1, MaxLegacyCoordinate));

    private static string Repeat(string sequence, int count) =>
        string.Concat(Enumerable.Repeat(sequence, count));
}
