using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class PaneLayoutMapper
{
    public static PaneLayout Capture(PaneNode node)
    {
        if (node.Pane is { } pane)
        {
            return new PaneLayout { Title = pane.Title, Weight = node.Weight };
        }

        return new PaneLayout
        {
            Orientation = node.Orientation == TerminalSplitOrientation.Vertical
                ? Constants.Layout.Vertical
                : Constants.Layout.Horizontal,
            Weight = node.Weight,
            Children = [.. node.Children.Select(Capture)],
        };
    }

    public static int Leaves(PaneLayout layout) =>
        layout.Children is { Count: > 0 } children ? children.Sum(Leaves) : 1;

    public static TerminalSplitOrientation Orientation(PaneLayout layout) =>
        layout.Orientation == Constants.Layout.Vertical
            ? TerminalSplitOrientation.Vertical
            : TerminalSplitOrientation.Horizontal;
}
