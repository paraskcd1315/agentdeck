using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class JsonRpcRequestDto
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = Constants.JsonRpcVersion;

    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("method")]
    public string Method { get; init; } = string.Empty;

    [JsonPropertyName("params")]
    public JsonNode? Params { get; init; }
}
