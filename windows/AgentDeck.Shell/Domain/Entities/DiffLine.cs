using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class DiffLine
{
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
