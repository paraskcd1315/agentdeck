using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class PanelButton
{
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; init; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; init; } = "live";
}
