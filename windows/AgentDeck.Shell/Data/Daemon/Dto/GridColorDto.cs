using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class GridColorDto
{
    [JsonPropertyName("k")]
    public string Kind { get; init; } = string.Empty;

    [JsonPropertyName("v")]
    public JsonElement? Value { get; init; }
}
