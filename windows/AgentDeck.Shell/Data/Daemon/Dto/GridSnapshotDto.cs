using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class GridSnapshotDto
{
    [JsonPropertyName("ptyId")]
    public long PtyId { get; init; }

    [JsonPropertyName("columns")]
    public int Columns { get; init; }

    [JsonPropertyName("rows")]
    public int Rows { get; init; }

    [JsonPropertyName("full")]
    public bool Full { get; init; }

    [JsonPropertyName("displayOffset")]
    public int DisplayOffset { get; init; }

    [JsonPropertyName("history")]
    public int History { get; init; }

    [JsonPropertyName("cursor")]
    public GridCursorDto? Cursor { get; init; }

    [JsonPropertyName("mode")]
    public GridModeDto? Mode { get; init; }

    [JsonPropertyName("lines")]
    public IReadOnlyList<GridLineDto> Lines { get; init; } = Array.Empty<GridLineDto>();
}
