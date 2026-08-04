using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class ShellConfig
{
    [JsonPropertyName("program")]
    public string? Program { get; init; }

    [JsonPropertyName("args")]
    public IReadOnlyList<string>? Args { get; init; }

    [JsonPropertyName("cwd")]
    public string? Cwd { get; init; }
}
