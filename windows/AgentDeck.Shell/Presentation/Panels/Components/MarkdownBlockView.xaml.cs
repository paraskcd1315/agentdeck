using AgentDeck.Shell.Domain.Entities;

using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class MarkdownBlockView : UserControl
{
    public MarkdownBlockView(MarkdownBlock block)
    {
        InitializeComponent();
        BodyText.Text = block.Text;
    }
}
