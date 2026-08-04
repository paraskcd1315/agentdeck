using System.ComponentModel;
using System.Runtime.CompilerServices;

using AgentDeck.Shell.Data.Daemon.Dto;
using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Domain.Interfaces;
using AgentDeck.Shell.Presentation.DesignSystem.TextGrid;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalViewModel : INotifyPropertyChanged
{
    private readonly IDaemonClient _client;
    private readonly IStringProvider _strings;
    private readonly ShellProfile _profile;
    private readonly SynchronizationContext? _uiContext;

    private long? _ptyId;
    private GridModeDto? _mode;
    private bool _started;
    private string _status = string.Empty;
    private int _columns = TerminalMetrics.DefaultCols;
    private int _rows = TerminalMetrics.DefaultRows;
    private int _displayOffset;
    private int _history;

    public TerminalViewModel(IDaemonClient client, IStringProvider strings, ShellProfile profile)
    {
        _client = client;
        _strings = strings;
        _profile = profile;
        _uiContext = SynchronizationContext.Current;
        _client.TerminalDamaged += OnTerminalDamaged;
        _client.HookEventReceived += OnHookEventReceived;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? GridChanged;

    public TextGridModel Grid { get; } = new();

    public bool UsesAlternateScreen => _mode?.AltScreen == true;

    public long? PtyId => _ptyId;

    public int Columns => _columns;

    public int Rows => _rows;

    public int DisplayOffset => _displayOffset;

    public int History => _history;

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_started)
        {
            return;
        }

        _started = true;
        Status = _strings.Get(StringKeys.TerminalStatusConnecting);

        if (!await _client.ConnectAsync(cancellationToken))
        {
            Status = _strings.Get(StringKeys.TerminalStatusDaemonUnavailable);
            return;
        }

        _ptyId = await _client.SpawnAsync(
            _profile.Program ?? Constants.Shell.FallbackProgram,
            _profile.Args ?? [],
            _profile.Cwd,
            ShellProfiles.Environment(_profile),
            _columns,
            _rows,
            cancellationToken);

        Status = _strings.Get(
            _ptyId is null ? StringKeys.TerminalStatusSpawnFailed : StringKeys.TerminalStatusReady);

        if (_ptyId is { } ptyId
            && await _client.SnapshotAsync(ptyId, cancellationToken) is { } snapshot)
        {
            ApplySnapshot(snapshot);
        }
    }

    public async Task SendAsync(string text, CancellationToken cancellationToken)
    {
        if (_ptyId is not { } ptyId)
        {
            return;
        }

        var stranded = _displayOffset > 0;
        await _client.WriteAsync(ptyId, text, cancellationToken);

        if (stranded)
        {
            await RefreshAsync(cancellationToken);
        }
    }

    public async Task ScrollPageAsync(int pages, CancellationToken cancellationToken)
    {
        if (_ptyId is not { } ptyId)
        {
            return;
        }

        var delta = pages * Math.Max(1, Grid.Rows - TerminalMetrics.PageOverlapLines);

        if (await _client.ScrollAsync(ptyId, delta, cancellationToken) is { } snapshot)
        {
            ApplySnapshot(snapshot);
        }
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        _client.TerminalDamaged -= OnTerminalDamaged;
        _client.HookEventReceived -= OnHookEventReceived;

        if (_ptyId is { } ptyId)
        {
            await _client.KillAsync(ptyId, cancellationToken);
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (_ptyId is not { } ptyId)
        {
            return;
        }

        if (await _client.SnapshotAsync(ptyId, cancellationToken) is { } snapshot)
        {
            ApplySnapshot(snapshot);
        }
    }

    public async Task WheelAsync(int notches, int columnIndex, int rowIndex, CancellationToken cancellationToken)
    {
        if (_ptyId is not { } ptyId)
        {
            return;
        }

        if (MouseEncoder.Wheel(_mode, notches, columnIndex, rowIndex) is { } sequence)
        {
            await _client.WriteAsync(ptyId, sequence, cancellationToken);
            return;
        }

        var delta = notches * TerminalMetrics.WheelScrollLines;

        if (await _client.ScrollAsync(ptyId, delta, cancellationToken) is { } snapshot)
        {
            ApplySnapshot(snapshot);
        }
    }

    public async Task ButtonAsync(
        TerminalMouseButton button,
        bool pressed,
        int columnIndex,
        int rowIndex,
        CancellationToken cancellationToken)
    {
        if (_ptyId is not { } ptyId)
        {
            return;
        }

        if (MouseEncoder.Button(_mode, button, pressed, columnIndex, rowIndex) is { } sequence)
        {
            await _client.WriteAsync(ptyId, sequence, cancellationToken);
        }
    }

    public async Task ResizeAsync(int columns, int rows, CancellationToken cancellationToken)
    {
        if (columns == _columns && rows == _rows)
        {
            return;
        }

        _columns = columns;
        _rows = rows;

        if (_ptyId is { } ptyId)
        {
            await _client.ResizeAsync(ptyId, columns, rows, cancellationToken);
            await RefreshAsync(cancellationToken);
        }
    }

    private void OnTerminalDamaged(object? sender, GridSnapshotEventArgs args)
    {
        if (_ptyId != args.Snapshot.PtyId)
        {
            return;
        }

        Post(() => ApplySnapshot(args.Snapshot));
    }

    private void ApplySnapshot(GridSnapshotDto snapshot)
    {
        _mode = snapshot.Mode ?? _mode;
        _displayOffset = snapshot.DisplayOffset;
        _history = snapshot.History;
        GridSnapshotMapper.Apply(Grid, snapshot);
        GridChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnHookEventReceived(object? sender, HookEventArgs args)
    {
        Post(() => Status = _strings.Format(StringKeys.TerminalStatusHookReceived, args.Payload));
    }

    private void Post(Action action)
    {
        if (_uiContext is null)
        {
            action();
            return;
        }

        _uiContext.Post(_ => action(), null);
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
