using AgentDeck.Shell.Domain.Entities;

using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class ActionsBlockView : UserControl
{
    private readonly Func<PanelButton, Task> _invoke;

    public ActionsBlockView(ActionsBlock block, Func<PanelButton, Task> invoke)
    {
        InitializeComponent();
        _invoke = invoke;

        foreach (var definition in block.Buttons)
        {
            Buttons.Children.Add(BuildButton(definition));
        }
    }

    private Button BuildButton(PanelButton definition)
    {
        var button = new Button { Content = definition.Label };
        button.Click += async (_, _) => await _invoke(definition);
        return button;
    }
}
