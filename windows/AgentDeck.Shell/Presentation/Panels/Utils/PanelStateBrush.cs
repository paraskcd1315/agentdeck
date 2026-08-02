using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class PanelStateBrush
{
    private const string Idle = "idle";
    private const string Working = "working";
    private const string Waiting = "waiting";

    public static Brush Resolve(string state) => state switch
    {
        Idle => new SolidColorBrush(Color.FromArgb(255, 0x4E, 0x7A, 0x4E)),
        Working => new SolidColorBrush(Color.FromArgb(255, 0xE5, 0xA8, 0x2B)),
        Waiting => new SolidColorBrush(Color.FromArgb(255, 0x4C, 0x8D, 0xFF)),
        _ => new SolidColorBrush(Color.FromArgb(255, 0xE5, 0x48, 0x4D)),
    };
}
