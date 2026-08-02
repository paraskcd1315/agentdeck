using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class ActionsBlock : PanelBlock
{
    [JsonPropertyName("buttons")]
    public IReadOnlyList<PanelButton> Buttons { get; init; } = Array.Empty<PanelButton>();
}
