using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class DiffBlock : PanelBlock
{
    [JsonPropertyName("file")]
    public string File { get; init; } = string.Empty;

    [JsonPropertyName("lines")]
    public IReadOnlyList<DiffLine> Lines { get; init; } = Array.Empty<DiffLine>();
}
