using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class GitStatus
{
    [JsonPropertyName("root")]
    public string Root { get; init; } = string.Empty;

    [JsonPropertyName("branch")]
    public string? Branch { get; init; }

    [JsonPropertyName("head")]
    public string? Head { get; init; }

    [JsonPropertyName("entries")]
    public IReadOnlyList<GitStatusEntry> Entries { get; init; } = Array.Empty<GitStatusEntry>();
}
