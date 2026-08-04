using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class PaneLayout
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("orientation")]
    public string? Orientation { get; init; }

    [JsonPropertyName("weight")]
    public double Weight { get; init; } = 1;

    [JsonPropertyName("children")]
    public IReadOnlyList<PaneLayout>? Children { get; init; }
}
