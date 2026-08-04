using System.Text.Json;
using System.Text.Json.Serialization;

using AgentDeck.Shell.Domain.Entities;

namespace AgentDeck.Shell.Data.Daemon.Json;

public sealed class PanelBlockConverter : JsonConverter<PanelBlock>
{
    private const string DiscriminatorProperty = "type";
    private const string MarkdownType = "markdown";
    private const string KeyValueType = "keyvalue";
    private const string StatusType = "status";
    private const string ActionsType = "actions";
    private const string TableType = "table";
    private const string DiffType = "diff";
    private const string StepsType = "steps";
    private const string LogType = "log";
    private const string ChartType = "chart";

    public override PanelBlock Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var raw = root.GetRawText();

        var type = root.TryGetProperty(DiscriminatorProperty, out var discriminator)
            ? discriminator.GetString()
            : null;

        return Typed(type, raw) ?? new UnknownBlock { Type = type ?? string.Empty, Raw = raw };
    }

    private static PanelBlock? Typed(string? type, string raw)
    {
        try
        {
            return type switch
            {
                MarkdownType => JsonSerializer.Deserialize<MarkdownBlock>(raw, PlainOptions),
                KeyValueType => JsonSerializer.Deserialize<KeyValueBlock>(raw, PlainOptions),
                StatusType => JsonSerializer.Deserialize<StatusBlock>(raw, PlainOptions),
                ActionsType => JsonSerializer.Deserialize<ActionsBlock>(raw, PlainOptions),
                TableType => JsonSerializer.Deserialize<TableBlock>(raw, PlainOptions),
                DiffType => JsonSerializer.Deserialize<DiffBlock>(raw, PlainOptions),
                StepsType => JsonSerializer.Deserialize<StepsBlock>(raw, PlainOptions),
                LogType => JsonSerializer.Deserialize<LogBlock>(raw, PlainOptions),
                ChartType => JsonSerializer.Deserialize<ChartBlock>(raw, PlainOptions),
                _ => null,
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, PanelBlock value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, value.GetType(), PlainOptions);

    private static JsonSerializerOptions PlainOptions { get; } = new();
}
