using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using AgentDeck.Shell.Data.Daemon.Dto;
using AgentDeck.Shell.Data.Daemon.Json;
using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Domain.Interfaces;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Data.Daemon;

public sealed class DaemonClient : IDaemonClient
{
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonRpcEnvelopeDto>> _pending = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly CancellationTokenSource _shutdown = new();

    private NamedPipeClientStream? _pipe;
    private StreamWriter? _writer;
    private Task? _reader;
    private long _nextId;

    public event EventHandler<PtyOutputEventArgs>? PtyOutputReceived;

    public event EventHandler<HookEventArgs>? HookEventReceived;

    public event EventHandler<PanelChangedEventArgs>? PanelChanged;

    public event EventHandler<GridSnapshotEventArgs>? TerminalDamaged;

    public bool IsConnected => _pipe?.IsConnected == true;

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            return await ConnectCoreAsync(cancellationToken);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task<bool> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        if (IsConnected)
        {
            return true;
        }

        var pipe = new NamedPipeClientStream(
            Constants.DaemonPipeServer,
            Constants.DaemonPipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        try
        {
            await pipe.ConnectAsync(Constants.DaemonConnectTimeoutMs, cancellationToken);
        }
        catch (Exception)
        {
            await pipe.DisposeAsync();
            return false;
        }

        _pipe = pipe;
        _writer = new StreamWriter(pipe, new UTF8Encoding(false)) { AutoFlush = true };
        _reader = Task.Run(() => ReadLoopAsync(pipe, _shutdown.Token), CancellationToken.None);
        return true;
    }

    public async Task<long?> SpawnAsync(
        string program,
        IReadOnlyList<string> args,
        string? cwd,
        IReadOnlyDictionary<string, string> env,
        int cols,
        int rows,
        CancellationToken cancellationToken)
    {
        var environment = new JsonObject();
        foreach (var entry in env)
        {
            environment[entry.Key] = entry.Value;
        }

        var parameters = new JsonObject
        {
            ["program"] = program,
            ["args"] = new JsonArray(args.Select(value => JsonValue.Create(value)).ToArray<JsonNode?>()),
            ["env"] = environment,
            ["cols"] = cols,
            ["rows"] = rows,
        };

        if (!string.IsNullOrWhiteSpace(cwd))
        {
            parameters["cwd"] = cwd;
        }

        var envelope = await CallAsync(Constants.Method.PtySpawn, parameters, cancellationToken);
        var result = envelope?.Result.Deserialize<SpawnResultDto>();
        return result?.PtyId;
    }

    public async Task KillAsync(long ptyId, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject { ["ptyId"] = ptyId };
        await CallAsync(Constants.Method.PtyKill, parameters, cancellationToken);
    }

    public async Task WriteAsync(long ptyId, string text, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject
        {
            ["ptyId"] = ptyId,
            ["dataB64"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(text)),
        };

        await CallAsync(Constants.Method.PtyWrite, parameters, cancellationToken);
    }

    public async Task ResizeAsync(long ptyId, int cols, int rows, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject
        {
            ["ptyId"] = ptyId,
            ["cols"] = cols,
            ["rows"] = rows,
        };

        await CallAsync(Constants.Method.PtyResize, parameters, cancellationToken);
    }

    public async Task<string?> OpenWorkspaceAsync(string path, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject { ["path"] = path };
        var envelope = await CallAsync(Constants.Method.WorkspaceOpen, parameters, cancellationToken);
        return envelope?.Result.Deserialize<WorkspaceOpenResultDto>()?.WorkspaceId;
    }

    public async Task<IReadOnlyList<string>> ListPanelsAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var parameters = new JsonObject { ["workspaceId"] = workspaceId };
        var envelope = await CallAsync(Constants.Method.PanelList, parameters, cancellationToken);
        var result = envelope?.Result.Deserialize<PanelListResultDto>();
        return result?.Panels.Select(panel => panel.Id).ToList() ?? [];
    }

    public async Task<PanelDefinition?> ReadPanelAsync(
        string workspaceId,
        string id,
        CancellationToken cancellationToken)
    {
        var parameters = new JsonObject { ["workspaceId"] = workspaceId, ["id"] = id };
        var envelope = await CallAsync(Constants.Method.PanelRead, parameters, cancellationToken);
        return envelope?.Result.Deserialize<PanelDefinition>(DaemonJson.Options);
    }

    public async Task<GitStatus?> GitStatusAsync(string path, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject { ["path"] = path };
        var envelope = await CallAsync(Constants.Method.GitStatus, parameters, cancellationToken);
        return envelope?.Result.Deserialize<GitStatus>(DaemonJson.Options);
    }

    public async Task<GridSnapshotDto?> SnapshotAsync(long ptyId, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject { ["ptyId"] = ptyId };
        var envelope = await CallAsync(Constants.Method.TerminalSnapshot, parameters, cancellationToken);
        return envelope?.Result.Deserialize<GridSnapshotDto>();
    }

    public async Task<GridSnapshotDto?> ScrollAsync(long ptyId, int delta, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject { ["ptyId"] = ptyId, ["delta"] = delta };
        var envelope = await CallAsync(Constants.Method.TerminalScroll, parameters, cancellationToken);
        return envelope?.Result.Deserialize<GridSnapshotDto>();
    }

    public async ValueTask DisposeAsync()
    {
        await _shutdown.CancelAsync();

        if (_writer is not null)
        {
            await _writer.DisposeAsync();
        }

        if (_pipe is not null)
        {
            await _pipe.DisposeAsync();
        }

        _shutdown.Dispose();
        _writeLock.Dispose();
        _connectLock.Dispose();
    }

    private async Task<JsonRpcEnvelopeDto?> CallAsync(
        string method,
        JsonNode parameters,
        CancellationToken cancellationToken)
    {
        if (_writer is null)
        {
            return null;
        }

        var id = Interlocked.Increment(ref _nextId);
        var request = new JsonRpcRequestDto { Id = id, Method = method, Params = parameters };
        var completion = new TaskCompletionSource<JsonRpcEnvelopeDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = completion;

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await _writer.WriteLineAsync(JsonSerializer.Serialize(request));
        }
        finally
        {
            _writeLock.Release();
        }

        using var registration = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        return await completion.Task;
    }

    private async Task ReadLoopAsync(Stream pipe, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(pipe, new UTF8Encoding(false));

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                return;
            }

            JsonRpcEnvelopeDto? envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<JsonRpcEnvelopeDto>(line);
            }
            catch (JsonException)
            {
                continue;
            }

            if (envelope is null)
            {
                continue;
            }

            if (envelope.IsNotification)
            {
                RaiseNotification(envelope);
                continue;
            }

            if (envelope.Id is { } id && _pending.TryRemove(id, out var completion))
            {
                completion.TrySetResult(envelope);
            }
        }
    }

    private void RaiseNotification(JsonRpcEnvelopeDto envelope)
    {
        switch (envelope.Method)
        {
            case Constants.Notification.PtyData:
                var data = envelope.Params.Deserialize<PtyDataDto>();
                if (data is not null)
                {
                    var text = Encoding.UTF8.GetString(Convert.FromBase64String(data.DataB64));
                    PtyOutputReceived?.Invoke(this, new PtyOutputEventArgs(data.PtyId, text));
                }

                break;

            case Constants.Notification.HookEvent:
                HookEventReceived?.Invoke(this, new HookEventArgs(envelope.Params?.ToJsonString() ?? string.Empty));
                break;

            case Constants.Notification.TerminalDamage:
                var damage = envelope.Params.Deserialize<GridSnapshotDto>();
                if (damage is not null)
                {
                    TerminalDamaged?.Invoke(this, new GridSnapshotEventArgs(damage));
                }

                break;

            case Constants.Notification.PanelChanged:
                var changed = envelope.Params.Deserialize<PanelChangedDto>();
                if (changed is not null)
                {
                    PanelChanged?.Invoke(this, new PanelChangedEventArgs(changed.WorkspaceId, changed.Id));
                }

                break;
        }
    }
}
