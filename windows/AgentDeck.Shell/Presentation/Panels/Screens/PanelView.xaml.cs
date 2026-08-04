using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Components;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Screens;

public sealed partial class PanelView : UserControl
{
    public PanelView(PanelDefinition panel, Func<PanelButton, Task> invoke)
    {
        InitializeComponent();
        TitleText.Text = panel.Title;

        if (panel.Schema != Constants.Panel.SchemaV1)
        {
            BlockHost.Children.Add(UnsupportedSchema(panel.Schema));
            return;
        }

        foreach (var block in panel.Blocks)
        {
            BlockHost.Children.Add(Build(block, invoke));
        }
    }

    private static UIElement Build(PanelBlock block, Func<PanelButton, Task> invoke) => block switch
    {
        MarkdownBlock markdown => new MarkdownBlockView(markdown),
        KeyValueBlock keyValue => new KeyValueBlockView(keyValue),
        StatusBlock status => new StatusBlockView(status),
        ActionsBlock actions => new ActionsBlockView(actions, invoke),
        TableBlock table => new TableBlockView(table),
        DiffBlock diff => new DiffBlockView(diff),
        StepsBlock steps => new StepsBlockView(steps),
        LogBlock log => new LogBlockView(log),
        ChartBlock chart => new ChartBlockView(chart),
        UnknownBlock unknown => new UnknownBlockView(unknown),
        _ => new UnknownBlockView(new UnknownBlock()),
    };

    private static TextBlock UnsupportedSchema(string schema) => new()
    {
        Text = AppServices.Strings.Format(StringKeys.PanelUnsupportedSchema, schema),
        TextWrapping = TextWrapping.Wrap,
        FontFamily = PanelResources.Font(PanelMetrics.FontUi),
        FontSize = PanelResources.Size(PanelMetrics.BodySize),
        Foreground = PanelResources.Brush(PanelMetrics.StateFailed),
    };
}
