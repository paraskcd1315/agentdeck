using AgentDeck.Shell.Presentation.Git.Utils;

namespace AgentDeck.Shell.Presentation.Workspace.ViewModels;

public sealed class ExplorerNode
{
    public string Label => StatusKind is { Length: > 0 } kind
        ? $"{GitStatusPalette.Marker(kind)}  {Name}"
        : Name;

    public required string Name { get; init; }

    public required string RelativePath { get; init; }

    public required bool IsDirectory { get; init; }

    public string? StatusKind { get; set; }

    public List<ExplorerNode> Children { get; } = [];
}
