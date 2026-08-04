using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class ChartSeries
{
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("points")]
    public IReadOnlyList<double> Points { get; init; } = Array.Empty<double>();
}
