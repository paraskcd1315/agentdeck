using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class GridSpanDto
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("fg")]
    public GridColorDto? Foreground { get; init; }

    [JsonPropertyName("bg")]
    public GridColorDto? Background { get; init; }

    [JsonPropertyName("bold")]
    public bool Bold { get; init; }

    [JsonPropertyName("italic")]
    public bool Italic { get; init; }

    [JsonPropertyName("underline")]
    public bool Underline { get; init; }

    [JsonPropertyName("inverse")]
    public bool Inverse { get; init; }

    [JsonPropertyName("strikeout")]
    public bool Strikeout { get; init; }
}
