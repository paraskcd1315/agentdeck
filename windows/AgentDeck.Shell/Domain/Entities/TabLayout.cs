using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class TabLayout
{
    [JsonPropertyName("profileId")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("root")]
    public PaneLayout? Root { get; init; }
}
