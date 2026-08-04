using System.Numerics;

using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;

namespace AgentDeck.Shell.Presentation.DesignSystem.Foundation;

public static class AdMotion
{
    private const string FastKey = "AdMotionFast";
    private const string OffsetTarget = "Offset";
    private const string FinalValue = "this.FinalValue";

    private static readonly Vector2 EaseFrom = new(0.32f, 0.72f);
    private static readonly Vector2 EaseTo = new(0, 1);

    public static void SlideOnReposition(UIElement element)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        var animation = compositor.CreateVector3KeyFrameAnimation();
        animation.Target = OffsetTarget;
        animation.Duration = TimeSpan.FromMilliseconds(PanelResources.Size(FastKey));
        animation.InsertExpressionKeyFrame(
            1,
            FinalValue,
            compositor.CreateCubicBezierEasingFunction(EaseFrom, EaseTo));

        var implicits = compositor.CreateImplicitAnimationCollection();
        implicits[OffsetTarget] = animation;
        visual.ImplicitAnimations = implicits;
    }
}
