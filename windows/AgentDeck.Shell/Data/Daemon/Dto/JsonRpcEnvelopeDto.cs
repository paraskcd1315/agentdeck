using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class JsonRpcEnvelopeDto
{
    [JsonPropertyName("id")]
    public long? Id { get; init; }

    [JsonPropertyName("method")]
    public string? Method { get; init; }

    [JsonPropertyName("params")]
    public JsonNode? Params { get; init; }

    [JsonPropertyName("result")]
    public JsonNode? Result { get; init; }

    [JsonPropertyName("error")]
    public JsonRpcErrorDto? Error { get; init; }

    public bool IsNotification => Id is null && Method is not null;
}
