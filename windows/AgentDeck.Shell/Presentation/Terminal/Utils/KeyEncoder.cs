using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;

using Windows.System;
using Windows.UI.Core;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class KeyEncoder
{
    private const string Escape = "";
    private const char Delete = '';

    public static string? Encode(KeyRoutedEventArgs args)
    {
        var control = IsDown(VirtualKey.Control);
        var shift = IsDown(VirtualKey.Shift);

        if (control && TryControlCombination(args.Key, out var combination))
        {
            return combination;
        }

        return args.Key switch
        {
            VirtualKey.Enter => "\r",
            VirtualKey.Back => Delete.ToString(),
            VirtualKey.Tab => shift ? $"{Escape}[Z" : "\t",
            VirtualKey.Escape => Escape,
            VirtualKey.Up => $"{Escape}[A",
            VirtualKey.Down => $"{Escape}[B",
            VirtualKey.Right => $"{Escape}[C",
            VirtualKey.Left => $"{Escape}[D",
            VirtualKey.Home => $"{Escape}[H",
            VirtualKey.End => $"{Escape}[F",
            VirtualKey.Insert => $"{Escape}[2~",
            VirtualKey.Delete => $"{Escape}[3~",
            VirtualKey.PageUp => $"{Escape}[5~",
            VirtualKey.PageDown => $"{Escape}[6~",
            VirtualKey.F1 => $"{Escape}OP",
            VirtualKey.F2 => $"{Escape}OQ",
            VirtualKey.F3 => $"{Escape}OR",
            VirtualKey.F4 => $"{Escape}OS",
            VirtualKey.F5 => $"{Escape}[15~",
            VirtualKey.F6 => $"{Escape}[17~",
            VirtualKey.F7 => $"{Escape}[18~",
            VirtualKey.F8 => $"{Escape}[19~",
            VirtualKey.F9 => $"{Escape}[20~",
            VirtualKey.F10 => $"{Escape}[21~",
            VirtualKey.F11 => $"{Escape}[23~",
            VirtualKey.F12 => $"{Escape}[24~",
            _ => null,
        };
    }

    public static bool IsPrintable(char character) =>
        character >= ' ' && character != Delete;

    private static bool TryControlCombination(VirtualKey key, out string? sequence)
    {
        if (key >= VirtualKey.A && key <= VirtualKey.Z)
        {
            sequence = ((char)(key - VirtualKey.A + 1)).ToString();
            return true;
        }

        sequence = key switch
        {
            VirtualKey.Space => "\0",
            _ => null,
        };

        return sequence is not null;
    }

    public static bool IsDown(VirtualKey key) =>
        InputKeyboardSource.GetKeyStateForCurrentThread(key).HasFlag(CoreVirtualKeyStates.Down);
}
