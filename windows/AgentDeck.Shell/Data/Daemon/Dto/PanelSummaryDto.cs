using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class PanelSummaryDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;
}
