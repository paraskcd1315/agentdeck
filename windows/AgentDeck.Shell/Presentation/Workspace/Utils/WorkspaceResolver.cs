using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Terminal.Utils;

namespace AgentDeck.Shell.Presentation.Workspace.Utils;

public static class WorkspaceResolver
{
    public static string Path(AppConfig config) =>
        string.IsNullOrWhiteSpace(config.Workspace?.Path)
            ? ShellResolver.Cwd(config.Shell)
            : config.Workspace.Path;
}
