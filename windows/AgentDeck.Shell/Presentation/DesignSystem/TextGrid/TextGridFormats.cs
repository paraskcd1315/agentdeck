using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Text;

using Windows.UI.Text;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed class TextGridFormats
{
    private readonly CanvasTextFormat _text;
    private readonly CanvasTextFormat _textBold;
    private readonly CanvasTextFormat _symbol;
    private readonly CanvasTextFormat _symbolBold;

    private TextGridFormats(
        CanvasTextFormat text,
        CanvasTextFormat textBold,
        CanvasTextFormat symbol,
        CanvasTextFormat symbolBold)
    {
        _text = text;
        _textBold = textBold;
        _symbol = symbol;
        _symbolBold = symbolBold;
    }

    public CanvasTextFormat Base => _text;

    public static TextGridFormats Create(string family, string symbolFamily, float size) =>
        new(
            Build(family, size, FontWeights.Normal),
            Build(family, size, FontWeights.Bold),
            Build(symbolFamily, size, FontWeights.Normal),
            Build(symbolFamily, size, FontWeights.Bold));

    public CanvasTextFormat For(bool symbol, bool bold) => (symbol, bold) switch
    {
        (true, true) => _symbolBold,
        (true, false) => _symbol,
        (false, true) => _textBold,
        (false, false) => _text,
    };

    public void ApplyLineSpacing(TextGridMetrics metrics)
    {
        foreach (var format in new[] { _text, _textBold, _symbol, _symbolBold })
        {
            format.LineSpacingMode = CanvasLineSpacingMode.Uniform;
            format.LineSpacing = (float)metrics.CellHeight;
            format.LineSpacingBaseline = (float)metrics.Baseline;
        }
    }

    private static CanvasTextFormat Build(string family, float size, FontWeight weight) => new()
    {
        FontFamily = family,
        FontSize = size,
        FontWeight = weight,
        WordWrapping = CanvasWordWrapping.NoWrap,
    };
}
