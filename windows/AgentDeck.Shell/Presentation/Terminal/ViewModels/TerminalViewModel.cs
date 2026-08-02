using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Domain.Interfaces;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class TerminalViewModel : INotifyPropertyChanged
{
    private readonly IDaemonClient _client;
    private readonly IStringProvider _strings;
    private readonly StringBuilder _buffer = new();
    private readonly SynchronizationContext? _uiContext;

    private long? _ptyId;
    private bool _started;
    private string _output = string.Empty;
    private string _status = string.Empty;

    public TerminalViewModel(IDaemonClient client, IStringProvider strings)
    {
        _client = client;
        _strings = strings;
        _uiContext = SynchronizationContext.Current;
        _client.PtyOutputReceived += OnPtyOutputReceived;
        _client.HookEventReceived += OnHookEventReceived;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Output
    {
        get => _output;
        private set => Set(ref _output, value);
    }

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public bool HasSession => _ptyId is not null;

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
            Constants.Shell.DefaultProgram,
            Array.Empty<string>(),
            TerminalMetrics.DefaultCols,
            TerminalMetrics.DefaultRows,
            cancellationToken);

        Status = _strings.Get(
            _ptyId is null ? StringKeys.TerminalStatusSpawnFailed : StringKeys.TerminalStatusReady);
    }

    public async Task SendAsync(string text, CancellationToken cancellationToken)
    {
        if (_ptyId is not { } ptyId)
        {
            return;
        }

        await _client.WriteAsync(ptyId, text, cancellationToken);
    }

    private void OnPtyOutputReceived(object? sender, PtyOutputEventArgs args)
    {
        if (_ptyId != args.PtyId)
        {
            return;
        }

        Post(() =>
        {
            _buffer.Append(AnsiFilter.Strip(args.Text));

            if (_buffer.Length > TerminalMetrics.MaxBufferCharacters)
            {
                _buffer.Remove(0, _buffer.Length - TerminalMetrics.TrimToCharacters);
            }

            Output = _buffer.ToString();
        });
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
