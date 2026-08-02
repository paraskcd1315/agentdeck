using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class StatusBlockView : UserControl
{
    public StatusBlockView(StatusBlock block)
    {
        InitializeComponent();
        StateDot.Fill = PanelStateBrush.Resolve(block.State);
        StateText.Text = block.Text;
    }
}
