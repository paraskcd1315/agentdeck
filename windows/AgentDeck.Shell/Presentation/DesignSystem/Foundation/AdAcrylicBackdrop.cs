using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace AgentDeck.Shell.Presentation.DesignSystem.Foundation;

public sealed partial class AdAcrylicBackdrop : SystemBackdrop
{
    private DesktopAcrylicController? _controller;
    private SystemBackdropConfiguration? _configuration;

    public DesktopAcrylicKind Kind { get; set; } = DesktopAcrylicKind.Thin;

    public Color? TintColor { get; set; }

    public double? TintOpacity { get; set; }

    public double? LuminosityOpacity { get; set; }

    public Color? FallbackColor { get; set; }

    public static bool IsSupported => DesktopAcrylicController.IsSupported();

    protected override void OnTargetConnected(
        ICompositionSupportsSystemBackdrop connectedTarget,
        XamlRoot xamlRoot)
    {
        base.OnTargetConnected(connectedTarget, xamlRoot);

        _controller ??= CreateController();
        _configuration ??= CreateConfiguration(xamlRoot);
        _controller.SetSystemBackdropConfiguration(_configuration);
        _controller.AddSystemBackdropTarget(connectedTarget);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        _controller?.RemoveSystemBackdropTarget(disconnectedTarget);
        _controller?.Dispose();
        _controller = null;
        _configuration = null;
    }

    private DesktopAcrylicController CreateController()
    {
        var controller = new DesktopAcrylicController { Kind = Kind };

        if (TintColor is { } tint)
        {
            controller.TintColor = tint;
        }

        if (TintOpacity is { } tintOpacity)
        {
            controller.TintOpacity = (float)tintOpacity;
        }

        if (LuminosityOpacity is { } luminosityOpacity)
        {
            controller.LuminosityOpacity = (float)luminosityOpacity;
        }

        if (FallbackColor is { } fallback)
        {
            controller.FallbackColor = fallback;
        }

        return controller;
    }

    private static SystemBackdropConfiguration CreateConfiguration(XamlRoot xamlRoot) => new()
    {
        IsInputActive = true,
        Theme = xamlRoot.Content is FrameworkElement { ActualTheme: ElementTheme.Light }
            ? SystemBackdropTheme.Light
            : SystemBackdropTheme.Dark,
    };
}
