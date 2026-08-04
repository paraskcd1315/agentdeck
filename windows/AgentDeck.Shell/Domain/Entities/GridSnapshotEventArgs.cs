using AgentDeck.Shell.Data.Daemon.Dto;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class GridSnapshotEventArgs : EventArgs
{
    public GridSnapshotEventArgs(GridSnapshotDto snapshot)
    {
        Snapshot = snapshot;
    }

    public GridSnapshotDto Snapshot { get; }
}
