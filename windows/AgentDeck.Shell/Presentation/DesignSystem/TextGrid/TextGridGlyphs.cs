namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public static class TextGridGlyphs
{
    private const char PrivateUseStart = '';
    private const char PrivateUseEnd = '';
    private const int WideCells = 2;
    private const int NarrowCells = 1;

    private static readonly (char Start, char End)[] WideRanges =
    [
        ('ᄀ', 'ᅟ'),
        ('⺀', '〾'),
        ('ぁ', '㏿'),
        ('㐀', '䶿'),
        ('一', '鿿'),
        ('ꀀ', '꓏'),
        ('가', '힣'),
        ('豈', '﫿'),
        ('︰', '﹯'),
        ('＀', '｠'),
        ('￠', '￦'),
    ];

    public static bool IsSymbol(char character) =>
        character >= PrivateUseStart && character <= PrivateUseEnd;

    public static bool IsWide(char character)
    {
        foreach (var range in WideRanges)
        {
            if (character >= range.Start && character <= range.End)
            {
                return true;
            }
        }

        return false;
    }

    public static int Cells(char character) => IsWide(character) ? WideCells : NarrowCells;

    public static int Width(string text)
    {
        var width = 0;

        foreach (var character in text)
        {
            width += Cells(character);
        }

        return width;
    }

    public static IEnumerable<TextGridSegment> Segments(string text, int column)
    {
        var index = 0;

        while (index < text.Length)
        {
            if (IsSymbol(text[index]) || IsWide(text[index]))
            {
                yield return new TextGridSegment(column, text[index].ToString(), IsSymbol(text[index]));
                column += Cells(text[index]);
                index++;
                continue;
            }

            var start = index;

            while (index < text.Length && !IsSymbol(text[index]) && !IsWide(text[index]))
            {
                index++;
            }

            var run = text[start..index];
            yield return new TextGridSegment(column, run, false);
            column += run.Length;
        }
    }
}
