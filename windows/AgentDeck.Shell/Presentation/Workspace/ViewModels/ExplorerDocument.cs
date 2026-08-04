using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Workspace.ViewModels;

public sealed class ExplorerDocument : WorkspaceDocument
{
    public ExplorerDocument(string root) => Root = root;

    public string Root { get; }

    public override string Title => AppServices.Strings.Get(StringKeys.ExplorerTitle);
}
