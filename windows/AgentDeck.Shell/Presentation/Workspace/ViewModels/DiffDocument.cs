namespace AgentDeck.Shell.Presentation.Workspace.ViewModels;

public sealed class DiffDocument : WorkspaceDocument
{
    public DiffDocument(string root, string file)
    {
        Root = root;
        File = file;
    }

    public string Root { get; }

    public string File { get; }

    public override string Title => System.IO.Path.GetFileName(File);
}
