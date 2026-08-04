using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class LogEntry
{
    [JsonPropertyName("level")]
    public string Level { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("time")]
    public string? Time { get; init; }
}
