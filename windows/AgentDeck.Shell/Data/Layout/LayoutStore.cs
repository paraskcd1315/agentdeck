using System.Text.Json;

using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Data.Layout;

public static class LayoutStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static WorkspaceLayout? Load()
    {
        try
        {
            var path = Path();
            return File.Exists(path)
                ? JsonSerializer.Deserialize<WorkspaceLayout>(File.ReadAllText(path), Options)
                : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void Save(WorkspaceLayout layout)
    {
        try
        {
            var path = Path();
            var temporary = $"{path}{Constants.Layout.TemporarySuffix}";

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            File.WriteAllText(temporary, JsonSerializer.Serialize(layout, Options));
            File.Move(temporary, path, true);
        }
        catch (Exception)
        {
        }
    }

    private static string Path() => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        Constants.StateDirectoryName,
        Constants.Layout.FileName);
}
