using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridMetrics
{
    private const string ReferenceGlyph = "M";
    private const double FallbackWidthRatio = 0.6;

    private TextGridMetrics(double cellWidth, double cellHeight, double baseline)
    {
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Baseline = baseline;
    }

    public double CellWidth { get; }

    public double CellHeight { get; }

    public double Baseline { get; }

    public static TextGridMetrics Measure(ICanvasResourceCreator device, CanvasTextFormat format)
    {
        using var layout = new CanvasTextLayout(device, ReferenceGlyph, format, 0f, 0f);

        var width = layout.LayoutBounds.Width > 0
            ? layout.LayoutBounds.Width
            : format.FontSize * FallbackWidthRatio;

        var metrics = layout.LineMetrics.Length > 0 ? layout.LineMetrics[0] : default;
        var height = metrics.Height > 0 ? metrics.Height : format.FontSize * 1.5;
        var baseline = metrics.Baseline > 0 ? metrics.Baseline : format.FontSize;

        return new TextGridMetrics(width, Math.Ceiling(height), baseline);
    }

    public int ColumnsFor(double width) => Math.Max(1, (int)(width / CellWidth));

    public int RowsFor(double height) => Math.Max(1, (int)(height / CellHeight));

    public int ColumnAt(double x) => Math.Max(0, (int)(x / CellWidth));

    public int RowAt(double y) => Math.Max(0, (int)(y / CellHeight));
}
