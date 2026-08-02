using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class KeyValueBlock : PanelBlock
{
    [JsonPropertyName("rows")]
    public IReadOnlyList<KeyValueRow> Rows { get; init; } = Array.Empty<KeyValueRow>();
}
