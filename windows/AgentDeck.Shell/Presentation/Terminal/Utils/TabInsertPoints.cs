using Windows.Foundation;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class TabInsertPoints
{
    public static int Resolve(IReadOnlyList<Rect> bounds, double x)
    {
        for (var index = 0; index < bounds.Count; index++)
        {
            var rect = bounds[index];

            if (x < rect.Left + (rect.Width / 2))
            {
                return index;
            }

            if (x < rect.Right)
            {
                return index + 1;
            }
        }

        return bounds.Count;
    }
}
