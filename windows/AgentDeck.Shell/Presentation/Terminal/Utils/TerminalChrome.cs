using AgentDeck.Shell.Presentation.Terminal.ViewModels;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class TerminalChrome
{
    private const string SessionSeparator = " — ";
    private const string PtyLabel = "pty ";
    private const string DimensionSeparator = "×";
    private const string PendingPty = "—";

    public static string Session(string title, long? ptyId, int index, int total)
    {
        var session = $"{title}{SessionSeparator}{PtyLabel}{ptyId?.ToString() ?? PendingPty}";
        return total > 1 ? $"{session}  {index}/{total}" : session;
    }

    public static string TabTitle(string title, int panes) =>
        panes > 1 ? $"{title}  {panes}" : title;

    public static string Dimensions(TerminalViewModel viewModel) =>
        $"{viewModel.Columns}{DimensionSeparator}{viewModel.Rows}";
}
