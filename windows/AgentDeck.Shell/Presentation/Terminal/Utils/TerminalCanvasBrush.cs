using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.DesignSystem.Foundation;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class TerminalCanvasBrush
{
    public static Brush Build(TerminalThemeConfig? config)
    {
        var opacity = config?.Opacity ?? Constants.Theme.DefaultTerminalOpacity;
        var blur = config?.Blur ?? Constants.Theme.DefaultTerminalBlur;
        var saturation = config?.Saturation ?? Constants.Theme.DefaultTerminalSaturation;
        var background = ColorParser.Parse(config?.Background) ?? DefaultBackground();

        if (blur <= 0)
        {
            return new SolidColorBrush(background) { Opacity = opacity };
        }

        return new BackdropGlassBrush
        {
            BlurAmount = blur,
            Saturation = saturation,
            TintColor = WithAlpha(background, opacity),
        };
    }

    private static Color DefaultBackground() => (Color)Application.Current.Resources["AdTermBg"];

    private static Color WithAlpha(Color color, double opacity) =>
        Color.FromArgb((byte)Math.Clamp(opacity * byte.MaxValue, 0, byte.MaxValue), color.R, color.G, color.B);
}
