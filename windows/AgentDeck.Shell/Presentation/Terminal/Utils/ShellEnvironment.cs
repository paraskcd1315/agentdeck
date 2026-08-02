namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class ShellEnvironment
{
    private const string PathVariable = "PATH";
    private const string TermVariable = "TERM";
    private const string ColorTermVariable = "COLORTERM";
    private const string TermProgramVariable = "TERM_PROGRAM";

    private const string TermValue = "xterm-256color";
    private const string ColorTermValue = "truecolor";
    private const string TermProgramValue = "AgentDeck";

    public static IReadOnlyDictionary<string, string> Build()
    {
        var environment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [TermVariable] = TermValue,
            [ColorTermVariable] = ColorTermValue,
            [TermProgramVariable] = TermProgramValue,
        };

        if (CurrentPath() is { Length: > 0 } path)
        {
            environment[PathVariable] = path;
        }

        return environment;
    }

    private static string CurrentPath()
    {
        var machine = Environment.GetEnvironmentVariable(PathVariable, EnvironmentVariableTarget.Machine);
        var user = Environment.GetEnvironmentVariable(PathVariable, EnvironmentVariableTarget.User);

        var segments = new[] { machine, user }
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToArray();

        return segments.Length > 0
            ? string.Join(Path.PathSeparator, segments)
            : Environment.GetEnvironmentVariable(PathVariable) ?? string.Empty;
    }
}
