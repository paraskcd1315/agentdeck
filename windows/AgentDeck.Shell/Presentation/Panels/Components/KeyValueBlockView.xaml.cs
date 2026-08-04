using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class KeyValueBlockView : UserControl
{
    public KeyValueBlockView(KeyValueBlock block)
    {
        InitializeComponent();

        foreach (var row in block.Rows)
        {
            Rows.Children.Add(BuildRow(row));
        }
    }

    private static Grid BuildRow(KeyValueRow row)
    {
        var grid = new Grid { ColumnSpacing = PanelMetrics.KeyValueColumnSpacing };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelMetrics.KeyColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var key = new TextBlock
        {
            Text = row.Key,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = PanelResources.Font(PanelMetrics.FontUi),
            FontSize = PanelResources.Size(PanelMetrics.BodySize),
            Foreground = PanelResources.Brush(PanelMetrics.TextSecondary),
        };

        var value = new TextBlock
        {
            Text = row.Value,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = PanelResources.Brush(PanelMetrics.TextPrimary),
        };

        Grid.SetColumn(value, 1);
        grid.Children.Add(key);
        grid.Children.Add(value);
        return grid;
    }
}
