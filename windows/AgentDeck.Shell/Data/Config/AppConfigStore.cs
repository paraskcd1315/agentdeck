using System.Text.Json;

using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Data.Config;

public static class AppConfigStore
{
    public static AppConfig Load()
    {
        var path = ConfigPath();

        if (!File.Exists(path))
        {
            return new AppConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path)) ?? new AppConfig();
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            return new AppConfig();
        }
    }

    public static string ConfigPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        Constants.StateDirectoryName,
        Constants.ConfigFileName);
}
