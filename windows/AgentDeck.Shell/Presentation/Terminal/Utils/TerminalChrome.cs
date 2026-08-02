using AgentDeck.Shell.Presentation.Terminal.ViewModels;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class TerminalChrome
{
    private const string SessionSeparator = " — ";
    private const string PtyLabel = "pty ";
    private const string DimensionSeparator = "×";
    private const string PendingPty = "—";

    public static string Session(TerminalTab tab) =>
        $"{tab.Title}{SessionSeparator}{PtyLabel}{tab.ViewModel.PtyId?.ToString() ?? PendingPty}";

    public static string Dimensions(TerminalViewModel viewModel) =>
        $"{viewModel.Columns}{DimensionSeparator}{viewModel.Rows}";
}
