using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class ChartBlockView : UserControl
{
    private const string LineKind = "line";

    private readonly ChartBlock _block;
    private readonly double _ceiling;

    public ChartBlockView(ChartBlock block)
    {
        InitializeComponent();

        _block = block;
        _ceiling = ChartGeometry.Ceiling(block.Series.Select(series => series.Points));

        if (block.Kind != LineKind)
        {
            BuildBars();
        }

        BuildLabels();
        BuildLegend();
    }

    private void OnPlotSizeChanged(object sender, SizeChangedEventArgs args)
    {
        if (_block.Kind != LineKind)
        {
            return;
        }

        Plot.Children.Clear();

        for (var index = 0; index < _block.Series.Count; index++)
        {
            Plot.Children.Add(BuildLine(_block.Series[index], index, args.NewSize));
        }
    }

    private Polyline BuildLine(ChartSeries series, int index, Windows.Foundation.Size size)
    {
        var line = new Polyline
        {
            Stroke = ChartPalette.Resolve(index),
            StrokeThickness = PanelMetrics.ChartLineThickness,
            StrokeLineJoin = PenLineJoin.Round,
        };

        foreach (var point in ChartGeometry.Line(series.Points, _ceiling, size.Width, size.Height))
        {
            line.Points.Add(point);
        }

        return line;
    }

    private void BuildBars()
    {
        var groups = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PanelMetrics.ChartBarSpacing,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var count = _block.Series.Select(series => series.Points.Count).DefaultIfEmpty(0).Max();

        for (var point = 0; point < count; point++)
        {
            groups.Children.Add(BuildGroup(point));
        }

        Plot.Children.Add(groups);
    }

    private StackPanel BuildGroup(int point)
    {
        var group = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 1,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        for (var index = 0; index < _block.Series.Count; index++)
        {
            var points = _block.Series[index].Points;
            var value = point < points.Count ? points[point] : 0;

            group.Children.Add(new Rectangle
            {
                Width = PanelMetrics.ChartBarSpacing,
                Height = ChartGeometry.BarHeight(value, _ceiling, PanelMetrics.ChartHeight),
                Fill = ChartPalette.Resolve(index),
                RadiusX = 1,
                RadiusY = 1,
                VerticalAlignment = VerticalAlignment.Bottom,
            });
        }

        return group;
    }

    private void BuildLabels()
    {
        foreach (var label in _block.Labels)
        {
            Labels.Children.Add(new TextBlock
            {
                Text = label,
                FontFamily = PanelResources.Font(PanelMetrics.FontUi),
                FontSize = PanelResources.Size(PanelMetrics.CaptionSize),
                Foreground = PanelResources.Brush(PanelMetrics.TextTertiary),
            });
        }
    }

    private void BuildLegend()
    {
        for (var index = 0; index < _block.Series.Count; index++)
        {
            Legend.Children.Add(BuildLegendEntry(_block.Series[index], index));
        }
    }

    private static StackPanel BuildLegendEntry(ChartSeries series, int index)
    {
        var entry = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PanelMetrics.StepRowSpacing,
        };

        entry.Children.Add(new Border
        {
            Width = PanelMetrics.StepDotSize,
            Height = PanelMetrics.StepDotSize,
            CornerRadius = new CornerRadius(PanelMetrics.StepDotSize / 2),
            Background = ChartPalette.Resolve(index),
            VerticalAlignment = VerticalAlignment.Center,
        });

        entry.Children.Add(new TextBlock
        {
            Text = series.Label,
            FontFamily = PanelResources.Font(PanelMetrics.FontUi),
            FontSize = PanelResources.Size(PanelMetrics.CaptionSize),
            Foreground = PanelResources.Brush(PanelMetrics.TextSecondary),
        });

        return entry;
    }
}
