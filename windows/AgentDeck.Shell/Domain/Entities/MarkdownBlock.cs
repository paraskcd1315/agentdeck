using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class MarkdownBlock : PanelBlock
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
