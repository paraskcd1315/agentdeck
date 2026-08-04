using Microsoft.UI.Xaml;

using Windows.UI;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public static class AnsiPalette
{
    private const int BaseColourCount = 16;
    private const int CubeStart = 16;
    private const int CubeSize = 216;
    private const int GreyStart = 232;
    private const int GreyCount = 24;
    private const int CubeAxis = 6;
    private const byte GreyBase = 8;
    private const byte GreyStep = 10;

    private static readonly string[] BaseKeys =
    [
        "AdAnsiBlack", "AdAnsiRed", "AdAnsiGreen", "AdAnsiYellow",
        "AdAnsiBlue", "AdAnsiMagenta", "AdAnsiCyan", "AdAnsiWhite",
        "AdAnsiBrightBlack", "AdAnsiBrightRed", "AdAnsiBrightGreen", "AdAnsiBrightYellow",
        "AdAnsiBrightBlue", "AdAnsiBrightMagenta", "AdAnsiBrightCyan", "AdAnsiBrightWhite",
    ];

    private static readonly byte[] CubeLevels = [0, 95, 135, 175, 215, 255];

    public static Color Indexed(int index) => index switch
    {
        >= 0 and < BaseColourCount => Resource(BaseKeys[index]),
        >= CubeStart and < CubeStart + CubeSize => Cube(index - CubeStart),
        >= GreyStart and < GreyStart + GreyCount => Grey(index - GreyStart),
        _ => Resource("AdTermFg"),
    };

    public static Color Foreground() => Resource("AdTermFg");

    public static Color Background() => Resource("AdTermBg");

    private static Color Cube(int offset)
    {
        var red = CubeLevels[offset / (CubeAxis * CubeAxis)];
        var green = CubeLevels[offset / CubeAxis % CubeAxis];
        var blue = CubeLevels[offset % CubeAxis];
        return Color.FromArgb(byte.MaxValue, red, green, blue);
    }

    private static Color Grey(int offset)
    {
        var level = (byte)(GreyBase + (GreyStep * offset));
        return Color.FromArgb(byte.MaxValue, level, level, level);
    }

    private static Color Resource(string key) => (Color)Application.Current.Resources[key];
}
