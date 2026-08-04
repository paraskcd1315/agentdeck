using System.Runtime.InteropServices;

using Windows.Graphics;

namespace AgentDeck.Shell.Data.Windows;

public static class CursorPosition
{
    public static PointInt32 Current() =>
        GetCursorPos(out var point) ? new PointInt32(point.X, point.Y) : new PointInt32(0, 0);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out NativePoint point);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }
}
