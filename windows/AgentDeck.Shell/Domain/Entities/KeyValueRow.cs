using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class KeyValueRow
{
    [JsonPropertyName("key")]
    public string Key { get; init; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}
