using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class AppConfig
{
    [JsonPropertyName("theme")]
    public ThemeConfig? Theme { get; init; }

    [JsonPropertyName("shell")]
    public ShellConfig? Shell { get; init; }

    [JsonPropertyName("workspace")]
    public WorkspaceConfig? Workspace { get; init; }

    [JsonPropertyName("profiles")]
    public IReadOnlyList<ShellProfile>? Profiles { get; init; }
}
