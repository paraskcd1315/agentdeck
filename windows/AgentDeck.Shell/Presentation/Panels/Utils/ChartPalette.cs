using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class ChartPalette
{
    private static readonly string[] Keys =
    [
        "AdBrandTextBrush",
        "AdStateIdleBrush",
        "AdStateWaitingBrush",
        "AdLogErrorBrush",
    ];

    public static Brush Resolve(int index) => PanelResources.Brush(Keys[index % Keys.Length]);
}
