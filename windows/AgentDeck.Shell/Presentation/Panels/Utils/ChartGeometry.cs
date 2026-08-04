using Windows.Foundation;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class ChartGeometry
{
    public static double Ceiling(IEnumerable<IReadOnlyList<double>> series)
    {
        var maximum = series.SelectMany(points => points).DefaultIfEmpty(0).Max();
        return maximum <= 0 ? 1 : maximum;
    }

    public static double BarHeight(double value, double ceiling, double height) =>
        Math.Max(value / ceiling * height, PanelMetrics.ChartMinimumBarHeight);

    public static IReadOnlyList<Point> Line(
        IReadOnlyList<double> points,
        double ceiling,
        double width,
        double height)
    {
        if (points.Count == 0 || width <= 0 || height <= 0)
        {
            return [];
        }

        var step = points.Count == 1 ? 0 : width / (points.Count - 1);

        return [.. points.Select((value, index) => new Point(
            points.Count == 1 ? width / 2 : index * step,
            height - (value / ceiling * height)))];
    }
}
