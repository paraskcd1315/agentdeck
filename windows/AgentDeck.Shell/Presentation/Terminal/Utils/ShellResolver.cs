using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class ShellResolver
{
    public static string Program(ShellConfig? config)
    {
        if (!string.IsNullOrWhiteSpace(config?.Program))
        {
            return config.Program;
        }

        return GitBash() ?? Constants.Shell.FallbackProgram;
    }

    public static IReadOnlyList<string> Args(ShellConfig? config)
    {
        if (config?.Args is { } configured)
        {
            return configured;
        }

        return string.IsNullOrWhiteSpace(config?.Program) && GitBash() is not null
            ? Constants.Shell.GitBashDefaultArgs
            : Array.Empty<string>();
    }

    public static string Cwd(ShellConfig? config) =>
        string.IsNullOrWhiteSpace(config?.Cwd)
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : config.Cwd;

    private static string? GitBash() => Constants.Shell.GitBashCandidates.FirstOrDefault(File.Exists);
}
