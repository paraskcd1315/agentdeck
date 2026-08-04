using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class FileDiffLine
{
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("old_line")]
    public int? OldLine { get; init; }

    [JsonPropertyName("new_line")]
    public int? NewLine { get; init; }
}
