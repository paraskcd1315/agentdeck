using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class PanelStep
{
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;
}
