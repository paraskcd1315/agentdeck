using System.Text;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class AnsiFilter
{
    private const char Escape = '';
    private const char Bell = '';
    private const char ControlSequenceIntroducer = '[';
    private const char OperatingSystemCommand = ']';
    private const char StringTerminator = '\\';

    public static string Strip(string value)
    {
        var builder = new StringBuilder(value.Length);
        var index = 0;

        while (index < value.Length)
        {
            var current = value[index];

            if (current != Escape)
            {
                if (current != '\r')
                {
                    builder.Append(current);
                }

                index++;
                continue;
            }

            index = SkipSequence(value, index);
        }

        return builder.ToString();
    }

    private static int SkipSequence(string value, int index)
    {
        var next = index + 1;
        if (next >= value.Length)
        {
            return value.Length;
        }

        return value[next] switch
        {
            ControlSequenceIntroducer => SkipControlSequence(value, next + 1),
            OperatingSystemCommand => SkipOperatingSystemCommand(value, next + 1),
            _ => next + 1,
        };
    }

    private static int SkipControlSequence(string value, int index)
    {
        while (index < value.Length && !char.IsLetter(value[index]))
        {
            index++;
        }

        return index < value.Length ? index + 1 : value.Length;
    }

    private static int SkipOperatingSystemCommand(string value, int index)
    {
        while (index < value.Length)
        {
            if (value[index] == Bell)
            {
                return index + 1;
            }

            if (value[index] == Escape && index + 1 < value.Length && value[index + 1] == StringTerminator)
            {
                return index + 2;
            }

            index++;
        }

        return value.Length;
    }
}
