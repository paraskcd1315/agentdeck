using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class UnknownBlockView : UserControl
{
    public UnknownBlockView(UnknownBlock block)
    {
        InitializeComponent();

        HeaderText.Text = block.Type.Length > 0
            ? AppServices.Strings.Format(StringKeys.PanelUnknownBlock, block.Type)
            : AppServices.Strings.Get(StringKeys.PanelUnknownBlockUntyped);

        RawText.Text = block.Raw;
    }
}
