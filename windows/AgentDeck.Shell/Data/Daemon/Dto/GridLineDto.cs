using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class GridLineDto
{
    [JsonPropertyName("line")]
    public int Line { get; init; }

    [JsonPropertyName("spans")]
    public IReadOnlyList<GridSpanDto> Spans { get; init; } = Array.Empty<GridSpanDto>();
}
