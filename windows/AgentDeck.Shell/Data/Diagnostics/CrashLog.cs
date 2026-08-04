using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Data.Diagnostics;

public static class CrashLog
{
    public static void Write(Exception exception)
    {
        try
        {
            var path = Path();
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            File.AppendAllText(path, $"{DateTimeOffset.Now:O}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception)
        {
        }
    }

    private static string Path() => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        Constants.StateDirectoryName,
        Constants.Diagnostics.CrashFileName);
}
