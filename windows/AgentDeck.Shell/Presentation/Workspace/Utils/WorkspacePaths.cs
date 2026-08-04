namespace AgentDeck.Shell.Presentation.Workspace.Utils;

public static class WorkspacePaths
{
    private const string HomeToken = "~";
    private const char PosixSeparator = '/';

    public static string Name(string path) =>
        new DirectoryInfo(path.TrimEnd(Path.DirectorySeparatorChar)).Name;

    public static string Display(string path)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var normalised = path.Replace(Path.DirectorySeparatorChar, PosixSeparator);

        if (!path.StartsWith(home, StringComparison.OrdinalIgnoreCase))
        {
            return normalised;
        }

        var relative = normalised[home.Length..].TrimStart(PosixSeparator);
        return relative.Length == 0 ? HomeToken : $"{HomeToken}{PosixSeparator}{relative}";
    }
}
