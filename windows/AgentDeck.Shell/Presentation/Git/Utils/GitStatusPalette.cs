using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Git.Utils;

public static class GitStatusPalette
{
    private const string Modified = "modified";
    private const string Added = "added";
    private const string Deleted = "deleted";
    private const string Renamed = "renamed";
    private const string TypeChanged = "typechanged";
    private const string Untracked = "untracked";

    public static Brush Brush(string kind) => PanelResources.Brush(kind switch
    {
        Modified => PanelMetrics.StateWorking,
        Added => PanelMetrics.DiffAddText,
        Deleted => PanelMetrics.DiffDelText,
        Renamed => PanelMetrics.StateWaiting,
        TypeChanged => PanelMetrics.StateWaiting,
        Untracked => PanelMetrics.TextTertiary,
        _ => PanelMetrics.StateFailed,
    });

    public static string Marker(string kind) => kind switch
    {
        Modified => "M",
        Added => "A",
        Deleted => "D",
        Renamed => "R",
        TypeChanged => "T",
        Untracked => "?",
        _ => "!",
    };
}
