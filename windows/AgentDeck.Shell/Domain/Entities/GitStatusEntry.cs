using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class GitStatusEntry
{
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("kind")]
    public string Kind { get; init; } = string.Empty;
}
