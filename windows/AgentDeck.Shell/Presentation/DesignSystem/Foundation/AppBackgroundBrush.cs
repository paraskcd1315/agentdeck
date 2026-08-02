using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using Windows.Foundation;

namespace AgentDeck.Shell.Presentation.DesignSystem.Foundation;

public static class AppBackgroundBrush
{
    public static Brush Build(ThemeConfig? theme)
    {
        var from = ColorParser.Parse(theme?.BackgroundGradientFrom ?? theme?.Surface);
        var to = ColorParser.Parse(theme?.BackgroundGradientTo ?? theme?.Background);

        if (from is null && to is null)
        {
            return (Brush)Application.Current.Resources[Constants.Theme.GradientAppKey];
        }

        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
        };

        brush.GradientStops.Add(new GradientStop
        {
            Offset = 0,
            Color = from ?? to!.Value,
        });

        brush.GradientStops.Add(new GradientStop
        {
            Offset = 0.78,
            Color = to ?? from!.Value,
        });

        return brush;
    }

    public static double Opacity(ThemeConfig? theme) =>
        theme?.BackgroundGradientOpacity ?? Constants.Theme.DefaultBackgroundGradientOpacity;
}
