using AgentDeck.Shell.Presentation.Workspace.Utils;

using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Workspace.Components;

public sealed partial class WorkspaceChip : UserControl
{
    public WorkspaceChip()
    {
        InitializeComponent();
    }

    public void Show(string path)
    {
        NameText.Text = WorkspacePaths.Name(path);
        PathText.Text = WorkspacePaths.Display(path);
    }
}
