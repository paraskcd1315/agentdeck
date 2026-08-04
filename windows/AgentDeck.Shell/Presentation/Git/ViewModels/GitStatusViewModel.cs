using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Domain.Interfaces;

namespace AgentDeck.Shell.Presentation.Git.ViewModels;

public sealed class GitStatusViewModel
{
    private readonly IDaemonClient _client;
    private readonly string _path;

    public GitStatusViewModel(IDaemonClient client, string path)
    {
        _client = client;
        _path = path;
    }

    public GitStatus? Status { get; private set; }

    public async Task<bool> RefreshAsync(CancellationToken cancellationToken)
    {
        Status = await _client.GitStatusAsync(_path, cancellationToken);
        return Status is not null;
    }
}
