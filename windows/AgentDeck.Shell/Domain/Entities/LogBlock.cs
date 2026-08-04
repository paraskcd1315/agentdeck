using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class LogBlock : PanelBlock
{
    [JsonPropertyName("entries")]
    public IReadOnlyList<LogEntry> Entries { get; init; } = Array.Empty<LogEntry>();
}
