using Microsoft.UI.Input;

namespace AgentDeck.Shell.Presentation.Terminal.Utils;

public static class PointerButtons
{
    public static TerminalMouseButton? Pressed(PointerPointProperties properties)
    {
        if (properties.IsLeftButtonPressed)
        {
            return TerminalMouseButton.Left;
        }

        if (properties.IsMiddleButtonPressed)
        {
            return TerminalMouseButton.Middle;
        }

        return properties.IsRightButtonPressed ? TerminalMouseButton.Right : null;
    }

    public static TerminalMouseButton? Released(PointerUpdateKind kind) => kind switch
    {
        PointerUpdateKind.LeftButtonReleased => TerminalMouseButton.Left,
        PointerUpdateKind.MiddleButtonReleased => TerminalMouseButton.Middle,
        PointerUpdateKind.RightButtonReleased => TerminalMouseButton.Right,
        _ => null,
    };
}
