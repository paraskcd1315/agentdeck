using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class PanelLogBrush
{
    private const string Trace = "trace";
    private const string Debug = "debug";
    private const string Info = "info";
    private const string Warn = "warn";
    private const string Error = "error";

    public static Brush Resolve(string level) => PanelResources.Brush(level switch
    {
        Trace => PanelMetrics.LogTrace,
        Debug => PanelMetrics.LogDebug,
        Info => PanelMetrics.LogInfo,
        Warn => PanelMetrics.LogWarn,
        Error => PanelMetrics.LogError,
        _ => PanelMetrics.LogFatal,
    });
}
