using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class LogBlockView : UserControl
{
    public LogBlockView(LogBlock block)
    {
        InitializeComponent();

        foreach (var entry in block.Entries)
        {
            Entries.Children.Add(BuildEntry(entry));
        }
    }

    private static Grid BuildEntry(LogEntry entry)
    {
        var grid = new Grid { ColumnSpacing = PanelMetrics.TableCellSpacing };
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(PanelMetrics.LogTimeColumnWidth),
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var time = new TextBlock
        {
            Text = entry.Time ?? string.Empty,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.CaptionSize),
            Foreground = PanelResources.Brush(PanelMetrics.TextTertiary),
        };

        var text = new TextBlock
        {
            Text = entry.Text,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = PanelLogBrush.Resolve(entry.Level),
        };

        Grid.SetColumn(text, 1);
        grid.Children.Add(time);
        grid.Children.Add(text);
        return grid;
    }
}
