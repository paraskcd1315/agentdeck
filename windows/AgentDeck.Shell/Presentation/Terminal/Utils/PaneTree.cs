using AgentDeck.Shell.Presentation.Terminal.ViewModels;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class PaneTree
{
    public static PaneNode Insert(
        PaneNode node,
        TerminalPane target,
        TerminalPane added,
        TerminalSplitOrientation orientation,
        bool before = false) =>
        Insert(node, target, PaneNode.Leaf(added), orientation, before);

    public static PaneNode Insert(
        PaneNode node,
        TerminalPane target,
        PaneNode added,
        TerminalSplitOrientation orientation,
        bool before = false)
    {
        if (node.IsLeaf)
        {
            if (!ReferenceEquals(node.Pane, target))
            {
                return node;
            }

            return before
                ? PaneNode.Split(orientation, added, node)
                : PaneNode.Split(orientation, node, added);
        }

        for (var index = 0; index < node.Children.Count; index++)
        {
            var child = node.Children[index];

            if (child.IsLeaf && ReferenceEquals(child.Pane, target)
                && node.Orientation == orientation)
            {
                node.Children.Insert(before ? index : index + 1, added);
                return node;
            }

            var replaced = Insert(child, target, added, orientation, before);

            if (!ReferenceEquals(replaced, child))
            {
                node.Children[index] = replaced;
                return node;
            }
        }

        return node;
    }

    public static PaneNode? Remove(PaneNode node, TerminalPane pane)
    {
        if (node.IsLeaf)
        {
            return ReferenceEquals(node.Pane, pane) ? null : node;
        }

        for (var index = node.Children.Count - 1; index >= 0; index--)
        {
            var replaced = Remove(node.Children[index], pane);

            if (replaced is null)
            {
                node.Children.RemoveAt(index);
                continue;
            }

            node.Children[index] = replaced;
        }

        return node.Children.Count switch
        {
            0 => null,
            1 => node.Children[0],
            _ => node,
        };
    }

    public static IEnumerable<TerminalPane> Leaves(PaneNode node)
    {
        if (node.Pane is { } pane)
        {
            yield return pane;
            yield break;
        }

        foreach (var child in node.Children)
        {
            foreach (var leaf in Leaves(child))
            {
                yield return leaf;
            }
        }
    }
}
