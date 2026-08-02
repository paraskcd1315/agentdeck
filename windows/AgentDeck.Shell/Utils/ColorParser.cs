using System.Globalization;

using Windows.UI;

namespace AgentDeck.Shell.Utils;

public static class ColorParser
{
    private const int RgbLength = 6;
    private const int ArgbLength = 8;

    public static Color? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var hex = value.TrimStart('#');

        return hex.Length switch
        {
            RgbLength => Build(byte.MaxValue, hex, 0),
            ArgbLength => Build(Component(hex, 0), hex, 2),
            _ => null,
        };
    }

    private static Color? Build(byte? alpha, string hex, int offset)
    {
        if (alpha is not { } a)
        {
            return null;
        }

        var red = Component(hex, offset);
        var green = Component(hex, offset + 2);
        var blue = Component(hex, offset + 4);

        if (red is not { } r || green is not { } g || blue is not { } b)
        {
            return null;
        }

        return Color.FromArgb(a, r, g, b);
    }

    private static byte? Component(string hex, int offset) =>
        byte.TryParse(hex.AsSpan(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}
