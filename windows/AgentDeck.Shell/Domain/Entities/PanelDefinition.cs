using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class PanelDefinition
{
    [JsonPropertyName("schema")]
    public string Schema { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("blocks")]
    public IReadOnlyList<PanelBlock> Blocks { get; init; } = Array.Empty<PanelBlock>();
}
