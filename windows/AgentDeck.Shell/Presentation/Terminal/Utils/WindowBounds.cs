using Windows.Graphics;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class WindowBounds
{
    public static bool Contains(RectInt32 bounds, PointInt32 point) =>
        point.X >= bounds.X
        && point.Y >= bounds.Y
        && point.X < bounds.X + bounds.Width
        && point.Y < bounds.Y + bounds.Height;
}
