using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class TerminalThemeConfig
{
    [JsonPropertyName("opacity")]
    public double? Opacity { get; init; }

    [JsonPropertyName("blur")]
    public double? Blur { get; init; }

    [JsonPropertyName("saturation")]
    public double? Saturation { get; init; }

    [JsonPropertyName("background")]
    public string? Background { get; init; }

    [JsonPropertyName("foreground")]
    public string? Foreground { get; init; }

    [JsonPropertyName("font")]
    public string? Font { get; init; }

    [JsonPropertyName("symbolFont")]
    public string? SymbolFont { get; init; }
}
