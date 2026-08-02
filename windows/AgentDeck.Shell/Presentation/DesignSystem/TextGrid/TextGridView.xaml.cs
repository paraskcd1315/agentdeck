using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Utils;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Foundation;
using Windows.UI;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed partial class TextGridView : UserControl
{
    private const float CursorOpacity = 0.75f;

    private TextGridFormats? _formats;
    private TextGridMetrics? _metrics;

    public TextGridView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    public event EventHandler<TextGridSize>? GridSizeChanged;

    public TextGridModel Model { get; set; } = new();

    public void Invalidate() => Surface.Invalidate();

    public int ColumnAt(double x) => _metrics?.ColumnAt(x) ?? 0;

    public int RowAt(double y) => _metrics?.RowAt(y) ?? 0;

    private void OnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        var terminal = AppServices.Config.Theme?.Terminal;
        var family = TextGridFonts.Resolve(terminal?.Font);
        var symbolFamily = TextGridFonts.ResolveSymbols(terminal?.SymbolFont);
        var size = (float)PanelResources.Size(PanelMetrics.MonoSize);

        _formats = TextGridFormats.Create(family, symbolFamily, size);
        _metrics = TextGridMetrics.Measure(sender, _formats.Base);
        _formats.ApplyLineSpacing(_metrics);
        ReportGridSize();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args) => ReportGridSize();

    private void ReportGridSize()
    {
        if (_metrics is not { } metrics || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        GridSizeChanged?.Invoke(this, new TextGridSize(
            metrics.ColumnsFor(ActualWidth),
            metrics.RowsFor(ActualHeight)));
    }

    private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (_formats is not { } formats || _metrics is not { } metrics)
        {
            return;
        }

        var session = args.DrawingSession;
        var background = AnsiPalette.Background();

        var visibleRows = Math.Min(Model.Rows, metrics.RowsFor(ActualHeight));

        for (var row = 0; row < visibleRows; row++)
        {
            var top = (float)Math.Round(row * metrics.CellHeight);
            var bottom = (float)Math.Round((row + 1) * metrics.CellHeight);
            var height = bottom - top;

            if (Model.Lines[row].Runs.Count == 0)
            {
                continue;
            }

            using var rowClip = session.CreateLayer(1f, new Rect(0, top, ActualWidth, height));

            foreach (var run in Model.Lines[row].Runs)
            {
                var left = (float)Math.Round(run.Column * metrics.CellWidth);
                var right = (float)Math.Round((run.Column + TextGridGlyphs.Width(run.Text)) * metrics.CellWidth);
                var width = right - left;

                if (!ColorsEqual(run.Style.Background, background))
                {
                    session.FillRectangle(left, top, width, height, run.Style.Background);
                }

                foreach (var segment in TextGridGlyphs.Segments(run.Text, run.Column))
                {
                    DrawSegment(session, segment, run.Style, top, metrics, formats);
                }

                if (run.Style.Underline)
                {
                    var baseline = bottom - 1f;
                    session.DrawLine(left, baseline, right, baseline, run.Style.Foreground);
                }
            }
        }

        DrawCursor(session, metrics);
    }

    private static void DrawSegment(
        CanvasDrawingSession session,
        TextGridSegment segment,
        TextGridStyle style,
        float top,
        TextGridMetrics metrics,
        TextGridFormats formats)
    {
        var format = formats.For(segment.IsSymbol, style.Bold);

        session.DrawText(
            segment.Text,
            (float)Math.Round(segment.Column * metrics.CellWidth),
            top,
            style.Foreground,
            format);
    }

    private void DrawCursor(CanvasDrawingSession session, TextGridMetrics metrics)
    {
        if (!Model.Cursor.Visible)
        {
            return;
        }

        var left = (float)Math.Round(Model.Cursor.Column * metrics.CellWidth);
        var right = (float)Math.Round((Model.Cursor.Column + 1) * metrics.CellWidth);
        var top = (float)Math.Round(Model.Cursor.Line * metrics.CellHeight);
        var bottom = (float)Math.Round((Model.Cursor.Line + 1) * metrics.CellHeight);

        var cursor = AnsiPalette.Foreground();
        cursor.A = (byte)(byte.MaxValue * CursorOpacity);

        session.FillRectangle(left, top, right - left, bottom - top, cursor);
    }

    private static bool ColorsEqual(Color left, Color right) =>
        left.A == right.A && left.R == right.R && left.G == right.G && left.B == right.B;
}
