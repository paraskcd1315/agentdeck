namespace AgentDeck.Shell.Presentation.Terminal.ViewModels;

public sealed class PaneNode
{
    private PaneNode(TerminalPane? pane, TerminalSplitOrientation orientation)
    {
        Pane = pane;
        Orientation = orientation;
    }

    public TerminalPane? Pane { get; }

    public TerminalSplitOrientation Orientation { get; }

    public List<PaneNode> Children { get; } = [];

    public bool IsLeaf => Pane is not null;

    public static PaneNode Leaf(TerminalPane pane) =>
        new(pane, TerminalSplitOrientation.Horizontal);

    public static PaneNode Split(TerminalSplitOrientation orientation, params PaneNode[] children)
    {
        var node = new PaneNode(null, orientation);
        node.Children.AddRange(children);
        return node;
    }
}
