using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class FileDiff
{
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("binary")]
    public bool Binary { get; init; }

    [JsonPropertyName("added")]
    public int Added { get; init; }

    [JsonPropertyName("removed")]
    public int Removed { get; init; }

    [JsonPropertyName("lines")]
    public IReadOnlyList<FileDiffLine> Lines { get; init; } = Array.Empty<FileDiffLine>();
}
