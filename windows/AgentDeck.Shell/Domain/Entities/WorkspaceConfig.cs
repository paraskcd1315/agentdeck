using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class WorkspaceConfig
{
    [JsonPropertyName("path")]
    public string? Path { get; init; }
}
