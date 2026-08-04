using Windows.Foundation;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class TabInsertPoints
{
    public static TabInsertPoint Resolve(IReadOnlyList<Rect> bounds, double x)
    {
        for (var index = 0; index < bounds.Count; index++)
        {
            var rect = bounds[index];

            if (x < rect.Left + (rect.Width / 2))
            {
                return new TabInsertPoint(index, rect.Left);
            }

            if (x < rect.Right)
            {
                return new TabInsertPoint(index + 1, rect.Right);
            }
        }

        return bounds.Count == 0
            ? new TabInsertPoint(0, 0)
            : new TabInsertPoint(bounds.Count, bounds[^1].Right);
    }
}
