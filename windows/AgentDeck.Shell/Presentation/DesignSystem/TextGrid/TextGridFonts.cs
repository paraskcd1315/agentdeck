using AgentDeck.Shell.Utils;

using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public static class TextGridFonts
{
    private const string MonoCandidatesKey = "AdFontMonoCandidates";
    private const string SymbolCandidatesKey = "AdFontSymbolCandidates";
    private const char CandidateSeparator = '|';
    private const string LastResort = "Consolas";

    public static string Resolve(string? preferred) => Resolve(preferred, MonoCandidatesKey);

    public static string ResolveSymbols(string? preferred) => Resolve(preferred, SymbolCandidatesKey);

    private static string Resolve(string? preferred, string candidatesKey)
    {
        var installed = CanvasTextFormat.GetSystemFontFamilies();

        foreach (var candidate in Candidates(preferred, candidatesKey))
        {
            if (installed.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return LastResort;
    }

    private static IEnumerable<string> Candidates(string? preferred, string candidatesKey)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            yield return preferred;
        }

        var configured = (string)Application.Current.Resources[candidatesKey];
        foreach (var candidate in configured.Split(CandidateSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            yield return candidate.Trim();
        }
    }
}
