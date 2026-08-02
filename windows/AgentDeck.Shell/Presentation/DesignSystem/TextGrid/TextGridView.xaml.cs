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

    private CanvasTextFormat? _format;
    private CanvasTextFormat? _boldFormat;
    private TextGridMetrics? _metrics;

    public TextGridView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    public event EventHandler<TextGridSize>? GridSizeChanged;

    public TextGridModel Model { get; set; } = new();

    public void Invalidate() => Surface.Invalidate();

    private void OnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        var family = TextGridFonts.Resolve(AppServices.Config.Theme?.Terminal?.Font);
        var size = (float)PanelResources.Size(PanelMetrics.MonoSize);

        _format = new CanvasTextFormat
        {
            FontFamily = family,
            FontSize = size,
            WordWrapping = CanvasWordWrapping.NoWrap,
        };

        _boldFormat = new CanvasTextFormat
        {
            FontFamily = family,
            FontSize = size,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            WordWrapping = CanvasWordWrapping.NoWrap,
        };

        _metrics = TextGridMetrics.Measure(sender, _format);
        ApplyLineSpacing(_format, _metrics);
        ApplyLineSpacing(_boldFormat, _metrics);
        ReportGridSize();
    }

    private static void ApplyLineSpacing(CanvasTextFormat format, TextGridMetrics metrics)
    {
        format.LineSpacingMode = CanvasLineSpacingMode.Uniform;
        format.LineSpacing = (float)metrics.CellHeight;
        format.LineSpacingBaseline = (float)metrics.Baseline;
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
        if (_format is not { } format || _boldFormat is not { } boldFormat || _metrics is not { } metrics)
        {
            return;
        }

        var session = args.DrawingSession;
        var background = AnsiPalette.Background();

        for (var row = 0; row < Model.Rows; row++)
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
                var right = (float)Math.Round((run.Column + run.Text.Length) * metrics.CellWidth);
                var width = right - left;

                if (!ColorsEqual(run.Style.Background, background))
                {
                    session.FillRectangle(left, top, width, height, run.Style.Background);
                }

                session.DrawText(
                    run.Text,
                    left,
                    top,
                    run.Style.Foreground,
                    run.Style.Bold ? boldFormat : format);

                if (run.Style.Underline)
                {
                    var baseline = bottom - 1f;
                    session.DrawLine(left, baseline, right, baseline, run.Style.Foreground);
                }
            }
        }

        DrawCursor(session, metrics);
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
