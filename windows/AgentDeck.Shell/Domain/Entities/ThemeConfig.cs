using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Domain.Entities;

public sealed class ThemeConfig
{
    [JsonPropertyName("mode")]
    public string? Mode { get; init; }

    [JsonPropertyName("background")]
    public string? Background { get; init; }

    [JsonPropertyName("surface")]
    public string? Surface { get; init; }

    [JsonPropertyName("brand")]
    public string? Brand { get; init; }

    [JsonPropertyName("backgroundGradientFrom")]
    public string? BackgroundGradientFrom { get; init; }

    [JsonPropertyName("backgroundGradientTo")]
    public string? BackgroundGradientTo { get; init; }

    [JsonPropertyName("backgroundGradientOpacity")]
    public double? BackgroundGradientOpacity { get; init; }

    [JsonPropertyName("glassBlur")]
    public double? GlassBlur { get; init; }

    [JsonPropertyName("glassOpacity")]
    public double? GlassOpacity { get; init; }

    [JsonPropertyName("terminal")]
    public TerminalThemeConfig? Terminal { get; init; }
}
