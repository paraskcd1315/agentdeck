using Windows.Foundation;
using Windows.Graphics;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class PassthroughRegions
{
    public static RectInt32[] Scale(IReadOnlyList<Rect> regions, double scale) =>
        [.. regions.Select(region => new RectInt32(
            (int)(region.X * scale),
            (int)(region.Y * scale),
            (int)(region.Width * scale),
            (int)(region.Height * scale)))];
}
