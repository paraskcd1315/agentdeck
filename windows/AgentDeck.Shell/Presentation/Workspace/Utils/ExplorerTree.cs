using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Workspace.ViewModels;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Workspace.Utils;

public static class ExplorerTree
{
    public static IReadOnlyList<ExplorerNode> Changed(WorkspaceStatus status)
    {
        var root = NewDirectory(string.Empty, string.Empty);

        foreach (var repository in status.Repositories)
        {
            foreach (var entry in repository.Entries)
            {
                var path = Join(repository.RelativeRoot, entry.Path);
                Place(root, path, entry.Kind);
            }
        }

        return root.Children;
    }

    public static IReadOnlyList<ExplorerNode> Directory(
        string absolute,
        string relative,
        IReadOnlyDictionary<string, string> statuses)
    {
        if (!System.IO.Directory.Exists(absolute))
        {
            return [];
        }

        var nodes = new List<ExplorerNode>();

        foreach (var directory in Entries(absolute, directories: true))
        {
            var path = Join(relative, directory);
            nodes.Add(NewDirectory(directory, path));
        }

        foreach (var file in Entries(absolute, directories: false))
        {
            var path = Join(relative, file);

            nodes.Add(new ExplorerNode
            {
                Name = file,
                RelativePath = path,
                IsDirectory = false,
                StatusKind = statuses.GetValueOrDefault(path),
            });
        }

        return nodes;
    }

    public static IReadOnlyDictionary<string, string> Statuses(WorkspaceStatus status)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var repository in status.Repositories)
        {
            foreach (var entry in repository.Entries)
            {
                map[Join(repository.RelativeRoot, entry.Path)] = entry.Kind;
            }
        }

        return map;
    }

    private static IEnumerable<string> Entries(string absolute, bool directories)
    {
        IEnumerable<string> paths;

        try
        {
            paths = directories
                ? System.IO.Directory.EnumerateDirectories(absolute)
                : System.IO.Directory.EnumerateFiles(absolute);
        }
        catch (Exception)
        {
            return [];
        }

        return paths
            .Select(System.IO.Path.GetFileName)
            .OfType<string>()
            .Where(name => !directories || !Constants.Explorer.SkippedDirectories.Contains(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
    }

    private static void Place(ExplorerNode root, string path, string kind)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = root;
        var walked = string.Empty;

        for (var index = 0; index < segments.Length; index++)
        {
            walked = Join(walked, segments[index]);
            var last = index == segments.Length - 1;

            var child = current.Children.FirstOrDefault(node => node.Name == segments[index]);

            if (child is null)
            {
                child = last
                    ? new ExplorerNode
                    {
                        Name = segments[index],
                        RelativePath = walked,
                        IsDirectory = false,
                        StatusKind = kind,
                    }
                    : NewDirectory(segments[index], walked);

                current.Children.Add(child);
            }

            current = child;
        }
    }

    private static ExplorerNode NewDirectory(string name, string path) => new()
    {
        Name = name,
        RelativePath = path,
        IsDirectory = true,
    };

    private static string Join(string left, string right) =>
        left.Length == 0 ? right : $"{left.TrimEnd('/')}/{right}";
}
