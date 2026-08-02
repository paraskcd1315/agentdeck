using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class StatusBlock : PanelBlock
{
    [JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
