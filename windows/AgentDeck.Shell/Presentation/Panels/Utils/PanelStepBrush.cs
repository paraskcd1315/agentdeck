using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class PanelStepBrush
{
    private const string Pending = "pending";
    private const string Active = "active";
    private const string Done = "done";

    public static Brush Resolve(string state) => PanelResources.Brush(state switch
    {
        Pending => PanelMetrics.TextTertiary,
        Active => PanelMetrics.StateWorking,
        Done => PanelMetrics.StateIdle,
        _ => PanelMetrics.StateFailed,
    });
}
