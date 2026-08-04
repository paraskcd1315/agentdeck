using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class StepsBlock : PanelBlock
{
    [JsonPropertyName("steps")]
    public IReadOnlyList<PanelStep> Steps { get; init; } = Array.Empty<PanelStep>();
}
