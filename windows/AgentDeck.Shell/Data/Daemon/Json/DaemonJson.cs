using System.Text.Json;

namespace AgentDeck.Shell.Data.Daemon.Json;

public static class DaemonJson
{
    public static JsonSerializerOptions Options { get; } = Build();

    private static JsonSerializerOptions Build()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new PanelBlockConverter());
        return options;
    }
}
