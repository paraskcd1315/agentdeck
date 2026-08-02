using Microsoft.UI.Xaml;

namespace AgentDeck.Shell.Presentation.Terminal.Components;

public static class TerminalTabMetrics
{
    public const double StripTopPadding = 8;
    public const double TabGap = 4;
    public const double DotSize = 8;
    public const double AddSize = 28;
    public const double AddBottomMargin = 4;
    public const double ChipDotSize = 16;

    public static readonly Thickness TabPadding = new(12, 8, 12, 8);
    public static readonly Thickness ChipPadding = new(12, 4, 12, 4);
    public static readonly CornerRadius TabRadius = new(8, 8, 0, 0);
    public static readonly CornerRadius ChipRadius = new(8);
    public static readonly CornerRadius DotRadius = new(4);
    public static readonly Thickness TabBorder = new(1, 1, 1, 0);
    public static readonly Thickness ContentGap = new(8, 0, 0, 0);
    public static readonly Thickness ClosePadding = new(4, 0, 0, 0);
    public static readonly Thickness ClosePad = new(3);
    public static readonly CornerRadius CloseRadius = new(4);

    public const double MaxLabelWidth = 140;
}
