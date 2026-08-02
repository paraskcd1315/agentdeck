using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class ShellProfiles
{
    public static IReadOnlyList<ShellProfile> Resolve(AppConfig config)
    {
        var configured = config.Profiles?.Where(profile => profile is not null).ToList();

        return configured is { Count: > 0 }
            ? configured.Select(Complete).ToList()
            : [FromShell(config.Shell)];
    }

    public static IReadOnlyList<ShellProfile> AutoStarting(IReadOnlyList<ShellProfile> profiles)
    {
        var starting = profiles.Where(profile => profile.AutoStart).ToList();
        return starting.Count > 0 ? starting : [profiles[0]];
    }

    public static IReadOnlyDictionary<string, string> Environment(ShellProfile profile)
    {
        var environment = new Dictionary<string, string>(ShellEnvironment.Build());

        if (profile.Env is { } overrides)
        {
            foreach (var entry in overrides)
            {
                environment[entry.Key] = entry.Value;
            }
        }

        return environment;
    }

    private static ShellProfile Complete(ShellProfile profile, int index)
    {
        var shell = new ShellConfig
        {
            Program = profile.Program,
            Args = profile.Args,
            Cwd = profile.Cwd,
        };

        return new ShellProfile
        {
            Id = profile.Id ?? $"{Constants.Shell.ProfileIdPrefix}{index}",
            Name = profile.Name ?? ShellResolver.Program(shell),
            Program = ShellResolver.Program(shell),
            Args = ShellResolver.Args(shell),
            Cwd = ShellResolver.Cwd(shell),
            Env = profile.Env,
            AutoStart = profile.AutoStart,
        };
    }

    private static ShellProfile FromShell(ShellConfig? shell) => new()
    {
        Id = Constants.Shell.DefaultProfileId,
        Name = Constants.Shell.DefaultProfileName,
        Program = ShellResolver.Program(shell),
        Args = ShellResolver.Args(shell),
        Cwd = ShellResolver.Cwd(shell),
        AutoStart = true,
    };
}
