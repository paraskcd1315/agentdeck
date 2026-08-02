using AgentDeck.Shell.Utils;

using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public static class TextGridFonts
{
    private const string CandidatesKey = "AdFontMonoCandidates";
    private const char CandidateSeparator = '|';
    private const string LastResort = "Consolas";

    public static string Resolve(string? preferred)
    {
        var installed = CanvasTextFormat.GetSystemFontFamilies();

        foreach (var candidate in Candidates(preferred))
        {
            if (installed.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return LastResort;
    }

    private static IEnumerable<string> Candidates(string? preferred)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            yield return preferred;
        }

        var configured = (string)Application.Current.Resources[CandidatesKey];
        foreach (var candidate in configured.Split(CandidateSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            yield return candidate.Trim();
        }
    }
}
