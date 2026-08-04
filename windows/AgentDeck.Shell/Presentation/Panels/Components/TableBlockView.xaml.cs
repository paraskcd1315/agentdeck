using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class TableBlockView : UserControl
{
    public TableBlockView(TableBlock block)
    {
        InitializeComponent();

        foreach (var _ in block.Columns)
        {
            Cells.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        Cells.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Cells.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (var column = 0; column < block.Columns.Count; column++)
        {
            Place(Header(block.Columns[column]), column, 0);
        }

        Place(Rule(block.Columns.Count), 0, 1);

        for (var row = 0; row < block.Rows.Count; row++)
        {
            Cells.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (var column = 0; column < block.Rows[row].Count; column++)
            {
                Place(Cell(block.Rows[row][column]), column, row + 2);
            }
        }
    }

    private void Place(FrameworkElement element, int column, int row)
    {
        Grid.SetColumn(element, column);
        Grid.SetRow(element, row);
        Cells.Children.Add(element);
    }

    private static TextBlock Header(string text) => new()
    {
        Text = text,
        FontFamily = PanelResources.Font(PanelMetrics.FontUi),
        FontSize = PanelResources.Size(PanelMetrics.CaptionSize),
        FontWeight = FontWeights.SemiBold,
        Foreground = PanelResources.Brush(PanelMetrics.TextSecondary),
    };

    private static TextBlock Cell(string text) => new()
    {
        Text = text,
        FontFamily = PanelResources.Font(PanelMetrics.FontMono),
        FontSize = PanelResources.Size(PanelMetrics.MonoSize),
        Foreground = PanelResources.Brush(PanelMetrics.TextPrimary),
    };

    private static Rectangle Rule(int columns)
    {
        var rule = new Rectangle
        {
            Height = 1,
            Fill = PanelResources.Brush(PanelMetrics.Stroke),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        Grid.SetColumnSpan(rule, Math.Max(columns, 1));
        return rule;
    }
}
