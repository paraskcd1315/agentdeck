using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class ChartBlock : PanelBlock
{
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = string.Empty;

    [JsonPropertyName("labels")]
    public IReadOnlyList<string> Labels { get; init; } = Array.Empty<string>();

    [JsonPropertyName("series")]
    public IReadOnlyList<ChartSeries> Series { get; init; } = Array.Empty<ChartSeries>();
}
