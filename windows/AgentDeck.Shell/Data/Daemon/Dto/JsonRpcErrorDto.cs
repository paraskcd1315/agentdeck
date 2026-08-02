using System.Text.Json.Serialization;

namespace AgentDeck.Shell.Data.Daemon.Dto;

public sealed class JsonRpcErrorDto
{
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
