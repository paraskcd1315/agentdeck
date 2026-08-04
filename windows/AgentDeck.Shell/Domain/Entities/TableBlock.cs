using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class TableBlock : PanelBlock
{
    [JsonPropertyName("columns")]
    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();

    [JsonPropertyName("rows")]
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = Array.Empty<IReadOnlyList<string>>();
}
