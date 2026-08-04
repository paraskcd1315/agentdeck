using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Git.Utils;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Workspace.Utils;
using AgentDeck.Shell.Presentation.Workspace.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Workspace.Components;

public sealed partial class ExplorerView : UserControl
{
    private readonly string _root;

    private IReadOnlyDictionary<string, string> _statuses = new Dictionary<string, string>();

    public ExplorerView(string root)
    {
        InitializeComponent();

        _root = root;
        RootText.Text = root;
        ModeText.Text = AppServices.Strings.Get(StringKeys.ExplorerAllFiles);

        Loaded += async (_, _) => await ReloadAsync();
    }

    public event EventHandler<string>? FileInvoked;

    public async Task ReloadAsync()
    {
        var status = await AppServices.Daemon.GitStatusAsync(_root, CancellationToken.None)
            ?? new WorkspaceStatus();

        _statuses = ExplorerTree.Statuses(status);
        Tree.RootNodes.Clear();

        var nodes = ChangedOnlySwitch.IsOn
            ? ExplorerTree.Changed(status)
            : ExplorerTree.Directory(_root, string.Empty, _statuses);

        foreach (var node in nodes)
        {
            Tree.RootNodes.Add(Build(node, 0));
        }
    }

    private TreeViewNode Build(ExplorerNode node, int depth)
    {
        var view = new TreeViewNode
        {
            Content = node,
            HasUnrealizedChildren = node.IsDirectory && node.Children.Count == 0,
            IsExpanded = node.IsDirectory && depth < Constants.Explorer.ExpandDepth,
        };

        foreach (var child in node.Children)
        {
            view.Children.Add(Build(child, depth + 1));
        }

        if (view.IsExpanded && view.HasUnrealizedChildren)
        {
            Realize(view);
        }

        return view;
    }

    private void Realize(TreeViewNode view)
    {
        if (view.Content is not ExplorerNode node || !view.HasUnrealizedChildren)
        {
            return;
        }

        view.HasUnrealizedChildren = false;

        var absolute = System.IO.Path.Combine(_root, node.RelativePath.Replace('/', '\\'));

        foreach (var child in ExplorerTree.Directory(absolute, node.RelativePath, _statuses))
        {
            view.Children.Add(new TreeViewNode
            {
                Content = child,
                HasUnrealizedChildren = child.IsDirectory,
            });
        }
    }

    private void OnItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is not TreeViewNode view || view.Content is not ExplorerNode node)
        {
            return;
        }

        if (!node.IsDirectory)
        {
            FileInvoked?.Invoke(this, node.RelativePath);
            return;
        }

        Realize(view);
        view.IsExpanded = !view.IsExpanded;
    }

    private async void OnModeToggled(object sender, RoutedEventArgs args)
    {
        ModeText.Text = AppServices.Strings.Get(ChangedOnlySwitch.IsOn
            ? StringKeys.ExplorerChangedOnly
            : StringKeys.ExplorerAllFiles);

        await ReloadAsync();
    }
}
