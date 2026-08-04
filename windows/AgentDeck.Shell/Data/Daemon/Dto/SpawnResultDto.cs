using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class SpawnResultDto
{
    [JsonPropertyName("ptyId")]
    public long PtyId { get; init; }
}
