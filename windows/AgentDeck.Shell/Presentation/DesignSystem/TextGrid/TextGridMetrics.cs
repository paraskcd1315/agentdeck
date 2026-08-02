using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridMetrics
{
    private const string ReferenceGlyph = "M";

    private TextGridMetrics(double cellWidth, double cellHeight, double baseline)
    {
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Baseline = baseline;
    }

    public double CellWidth { get; }

    public double CellHeight { get; }

    public double Baseline { get; }

    public static TextGridMetrics Measure(
        ICanvasResourceCreator device,
        CanvasTextFormat format,
        double lineHeight)
    {
        using var layout = new CanvasTextLayout(device, ReferenceGlyph, format, 0f, 0f);
        var width = layout.LayoutBounds.Width > 0 ? layout.LayoutBounds.Width : format.FontSize * 0.6;
        var baseline = layout.LineMetrics.Length > 0 ? layout.LineMetrics[0].Baseline : format.FontSize;
        return new TextGridMetrics(width, lineHeight, baseline);
    }

    public int ColumnsFor(double width) => Math.Max(1, (int)(width / CellWidth));

    public int RowsFor(double height) => Math.Max(1, (int)(height / CellHeight));
}
