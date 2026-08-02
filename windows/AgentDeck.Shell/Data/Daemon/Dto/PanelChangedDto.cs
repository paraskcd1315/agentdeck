using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class PanelChangedDto
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; init; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
}
