using AgentDeck.Shell.Data.Daemon.Dto;
using AgentDeck.Shell.Domain.Entities;

namespace AgentDeck.Shell.Domain.Interfaces;

public interface IDaemonClient : IAsyncDisposable
{
    event EventHandler<PtyOutputEventArgs>? PtyOutputReceived;

    event EventHandler<HookEventArgs>? HookEventReceived;

    event EventHandler<PanelChangedEventArgs>? PanelChanged;

    event EventHandler<GridSnapshotEventArgs>? TerminalDamaged;

    bool IsConnected { get; }

    Task<bool> ConnectAsync(CancellationToken cancellationToken);

    Task<long?> SpawnAsync(string program, IReadOnlyList<string> args, int cols, int rows, CancellationToken cancellationToken);

    Task WriteAsync(long ptyId, string text, CancellationToken cancellationToken);

    Task ResizeAsync(long ptyId, int cols, int rows, CancellationToken cancellationToken);

    Task<string?> OpenWorkspaceAsync(string path, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListPanelsAsync(string workspaceId, CancellationToken cancellationToken);

    Task<PanelDefinition?> ReadPanelAsync(string workspaceId, string id, CancellationToken cancellationToken);

    Task<GridSnapshotDto?> SnapshotAsync(long ptyId, CancellationToken cancellationToken);
}
