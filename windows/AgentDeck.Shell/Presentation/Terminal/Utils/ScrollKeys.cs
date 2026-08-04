using Microsoft.UI.Xaml.Input;

using Windows.System;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class ScrollKeys
{
    public static int? Pages(KeyRoutedEventArgs args)
    {
        if (!KeyEncoder.IsDown(VirtualKey.Shift))
        {
            return null;
        }

        return args.Key switch
        {
            VirtualKey.PageUp => 1,
            VirtualKey.PageDown => -1,
            _ => null,
        };
    }
}
