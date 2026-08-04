namespace AgentDeck.Shell.Domain.Entities;

public sealed class PanelChangedEventArgs : EventArgs
{
    public PanelChangedEventArgs(string workspaceId, string id)
    {
        WorkspaceId = workspaceId;
        Id = id;
    }

    public string WorkspaceId { get; }

    public string Id { get; }
}
