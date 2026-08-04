using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class WorkspaceOpenResultDto
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; init; } = string.Empty;

    [JsonPropertyName("panelsPath")]
    public string PanelsPath { get; init; } = string.Empty;
}
