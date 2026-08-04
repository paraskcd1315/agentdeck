using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class GridModeDto
{
    [JsonPropertyName("altScreen")]
    public bool AltScreen { get; init; }

    [JsonPropertyName("applicationCursor")]
    public bool ApplicationCursor { get; init; }

    [JsonPropertyName("mouseReport")]
    public bool MouseReport { get; init; }

    [JsonPropertyName("sgrMouse")]
    public bool SgrMouse { get; init; }

    [JsonPropertyName("alternateScroll")]
    public bool AlternateScroll { get; init; }
}
