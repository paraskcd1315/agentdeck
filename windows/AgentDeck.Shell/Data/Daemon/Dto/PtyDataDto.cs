using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class PtyDataDto
{
    [JsonPropertyName("ptyId")]
    public long PtyId { get; init; }

    [JsonPropertyName("dataB64")]
    public string DataB64 { get; init; } = string.Empty;
}
