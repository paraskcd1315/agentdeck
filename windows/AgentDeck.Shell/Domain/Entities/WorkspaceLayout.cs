using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class WorkspaceLayout
{
    [JsonPropertyName("panelWidth")]
    public double PanelWidth { get; init; }

    [JsonPropertyName("tabs")]
    public IReadOnlyList<TabLayout> Tabs { get; init; } = [];
}
