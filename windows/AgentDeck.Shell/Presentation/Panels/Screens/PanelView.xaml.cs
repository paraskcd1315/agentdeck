using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Components;

using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Screens;

public sealed partial class PanelView : UserControl
{
    public PanelView(PanelDefinition panel, Func<PanelButton, Task> invoke)
    {
        InitializeComponent();
        TitleText.Text = panel.Title;

        foreach (var block in panel.Blocks)
        {
            var view = Build(block, invoke);
            if (view is not null)
            {
                BlockHost.Children.Add(view);
            }
        }
    }

    private static UserControl? Build(PanelBlock block, Func<PanelButton, Task> invoke) => block switch
    {
        MarkdownBlock markdown => new MarkdownBlockView(markdown),
        KeyValueBlock keyValue => new KeyValueBlockView(keyValue),
        StatusBlock status => new StatusBlockView(status),
        ActionsBlock actions => new ActionsBlockView(actions, invoke),
        _ => null,
    };
}
