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

    public override PanelBlock? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (!root.TryGetProperty(DiscriminatorProperty, out var discriminator))
        {
            return null;
        }

        var raw = root.GetRawText();

        return discriminator.GetString() switch
        {
            MarkdownType => JsonSerializer.Deserialize<MarkdownBlock>(raw, PlainOptions),
            KeyValueType => JsonSerializer.Deserialize<KeyValueBlock>(raw, PlainOptions),
            StatusType => JsonSerializer.Deserialize<StatusBlock>(raw, PlainOptions),
            ActionsType => JsonSerializer.Deserialize<ActionsBlock>(raw, PlainOptions),
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, PanelBlock value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, value.GetType(), PlainOptions);

    private static JsonSerializerOptions PlainOptions { get; } = new();
}
