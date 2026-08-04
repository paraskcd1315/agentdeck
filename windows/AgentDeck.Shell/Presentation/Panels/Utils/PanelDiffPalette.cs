using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class PanelDiffPalette
{
    private const string Add = "add";
    private const string Delete = "delete";
    private const string Hunk = "hunk";

    public static Brush? Background(string kind) => kind switch
    {
        Add => PanelResources.Brush(PanelMetrics.DiffAddBackground),
        Delete => PanelResources.Brush(PanelMetrics.DiffDelBackground),
        _ => null,
    };

    public static Brush Foreground(string kind) => PanelResources.Brush(kind switch
    {
        Add => PanelMetrics.DiffAddText,
        Delete => PanelMetrics.DiffDelText,
        Hunk => PanelMetrics.DiffHunkText,
        _ => PanelMetrics.TextSecondary,
    });

    public static string Marker(string kind) => kind switch
    {
        Add => "+",
        Delete => "-",
        Hunk => "@",
        _ => " ",
    };
}
