using System.Text.Json;

using AgentDeck.Shell.Data.Daemon.Dto;
using AgentDeck.Shell.Presentation.DesignSystem.TextGrid;
using AgentDeck.Shell.Utils;

using Windows.UI;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class GridSnapshotMapper
{
    private const string KindIndex = "idx";
    private const string KindRgb = "rgb";
    private const string KindForeground = "fg";

    public static void Apply(TextGridModel model, GridSnapshotDto snapshot)
    {
        model.Resize(snapshot.Columns, snapshot.Rows);

        foreach (var line in snapshot.Lines)
        {
            model.SetRow(line.Line, MapRuns(line.Spans));
        }

        if (snapshot.Cursor is { } cursor)
        {
            model.Cursor.Line = cursor.Line;
            model.Cursor.Column = cursor.Column;
            model.Cursor.Visible = cursor.Visible;
        }
    }

    private static List<TextGridRun> MapRuns(IReadOnlyList<GridSpanDto> spans)
    {
        var runs = new List<TextGridRun>(spans.Count);
        var column = 0;

        foreach (var span in spans)
        {
            var foreground = Resolve(span.Foreground, AnsiPalette.Foreground());
            var background = Resolve(span.Background, AnsiPalette.Background());

            if (span.Inverse)
            {
                (foreground, background) = (background, foreground);
            }

            runs.Add(new TextGridRun
            {
                Column = column,
                Text = span.Text,
                Style = new TextGridStyle
                {
                    Foreground = foreground,
                    Background = background,
                    Bold = span.Bold,
                    Italic = span.Italic,
                    Underline = span.Underline,
                    Strikeout = span.Strikeout,
                },
            });

            column += span.Text.Length;
        }

        return runs;
    }

    private static Color Resolve(GridColorDto? color, Color fallback)
    {
        if (color is null)
        {
            return fallback;
        }

        return color.Kind switch
        {
            KindIndex when color.Value is { ValueKind: JsonValueKind.Number } value =>
                AnsiPalette.Indexed(value.GetInt32()),
            KindRgb when color.Value is { ValueKind: JsonValueKind.String } value =>
                ColorParser.Parse(value.GetString()) ?? fallback,
            KindForeground => AnsiPalette.Foreground(),
            _ => fallback,
        };
    }
}
