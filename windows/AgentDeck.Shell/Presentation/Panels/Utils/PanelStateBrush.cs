using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class PanelStateBrush
{
    private const string Idle = "idle";
    private const string Working = "working";
    private const string Waiting = "waiting";

    public static Brush Resolve(string state) => PanelResources.Brush(state switch
    {
        Idle => PanelMetrics.StateIdle,
        Working => PanelMetrics.StateWorking,
        Waiting => PanelMetrics.StateWaiting,
        _ => PanelMetrics.StateFailed,
    });
}
