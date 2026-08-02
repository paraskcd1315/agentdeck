using AgentDeck.Shell.Domain.Entities;

namespace AgentDeck.Shell.Domain.Interfaces;

public interface IDaemonClient : IAsyncDisposable
{
    event EventHandler<PtyOutputEventArgs>? PtyOutputReceived;

    event EventHandler<HookEventArgs>? HookEventReceived;

    bool IsConnected { get; }

    Task<bool> ConnectAsync(CancellationToken cancellationToken);

    Task<long?> SpawnAsync(string program, IReadOnlyList<string> args, int cols, int rows, CancellationToken cancellationToken);

    Task WriteAsync(long ptyId, string text, CancellationToken cancellationToken);

    Task ResizeAsync(long ptyId, int cols, int rows, CancellationToken cancellationToken);
}
