using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class PanelListResultDto
{
    [JsonPropertyName("panels")]
    public IReadOnlyList<PanelSummaryDto> Panels { get; init; } = Array.Empty<PanelSummaryDto>();
}
