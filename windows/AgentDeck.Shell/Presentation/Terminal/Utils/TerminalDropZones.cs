using AgentDeck.Shell.Presentation.Terminal.ViewModels;

using Windows.Foundation;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class TerminalDropZones
{
    public static TerminalDropZone Resolve(Point position, double width, double height)
    {
        if (width <= 0 || height <= 0)
        {
            return new TerminalDropZone(TerminalSplitOrientation.Horizontal, false);
        }

        var horizontal = (position.X / width) - 0.5;
        var vertical = (position.Y / height) - 0.5;

        return Math.Abs(horizontal) >= Math.Abs(vertical)
            ? new TerminalDropZone(TerminalSplitOrientation.Horizontal, horizontal < 0)
            : new TerminalDropZone(TerminalSplitOrientation.Vertical, vertical < 0);
    }
}
