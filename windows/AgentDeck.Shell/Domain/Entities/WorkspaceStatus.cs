using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class WorkspaceStatus
{
    [JsonPropertyName("root")]
    public string Root { get; init; } = string.Empty;

    [JsonPropertyName("repositories")]
    public IReadOnlyList<RepositoryStatus> Repositories { get; init; } = Array.Empty<RepositoryStatus>();
}
