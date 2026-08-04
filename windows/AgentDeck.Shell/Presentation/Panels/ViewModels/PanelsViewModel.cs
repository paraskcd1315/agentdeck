using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Domain.Interfaces;

namespace AgentDeck.Shell.Presentation.Panels.ViewModels;

public sealed class PanelsViewModel
{
    private readonly IDaemonClient _client;
    private readonly SynchronizationContext? _uiContext;

    private string? _workspaceId;

    public PanelsViewModel(IDaemonClient client)
    {
        _client = client;
        _uiContext = SynchronizationContext.Current;
        _client.PanelChanged += OnPanelChanged;
    }

    public event EventHandler<IReadOnlyList<PanelDefinition>>? PanelsLoaded;

    public async Task OpenAsync(string workspacePath, CancellationToken cancellationToken)
    {
        if (!await _client.ConnectAsync(cancellationToken))
        {
            return;
        }

        _workspaceId = await _client.OpenWorkspaceAsync(workspacePath, cancellationToken);
        await ReloadAsync(cancellationToken);
    }

    public async Task ReloadAsync(CancellationToken cancellationToken)
    {
        if (_workspaceId is not { } workspaceId)
        {
            return;
        }

        var ids = await _client.ListPanelsAsync(workspaceId, cancellationToken);
        var panels = new List<PanelDefinition>(ids.Count);

        foreach (var id in ids)
        {
            var panel = await _client.ReadPanelAsync(workspaceId, id, cancellationToken);
            if (panel is not null)
            {
                panels.Add(panel);
            }
        }

        Post(() => PanelsLoaded?.Invoke(this, panels));
    }

    private void OnPanelChanged(object? sender, PanelChangedEventArgs args)
    {
        if (args.WorkspaceId != _workspaceId)
        {
            return;
        }

        _ = ReloadAsync(CancellationToken.None);
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
}
