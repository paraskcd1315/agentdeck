using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class DiffBlockView : UserControl
{
    public DiffBlockView(DiffBlock block)
    {
        InitializeComponent();
        FileText.Text = block.File;

        foreach (var line in block.Lines)
        {
            Lines.Children.Add(BuildLine(line));
        }
    }

    private static Border BuildLine(DiffLine line)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(PanelMetrics.DiffGutterWidth),
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var foreground = PanelDiffPalette.Foreground(line.Kind);

        var marker = new TextBlock
        {
            Text = PanelDiffPalette.Marker(line.Kind),
            TextAlignment = TextAlignment.Center,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = foreground,
        };

        var text = new TextBlock
        {
            Text = line.Text,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = foreground,
        };

        Grid.SetColumn(text, 1);
        grid.Children.Add(marker);
        grid.Children.Add(text);

        return new Border
        {
            Child = grid,
            Background = PanelDiffPalette.Background(line.Kind),
        };
    }
}
