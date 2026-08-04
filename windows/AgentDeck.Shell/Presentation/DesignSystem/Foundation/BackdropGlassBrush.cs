using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace AgentDeck.Shell.Presentation.DesignSystem.Foundation;

public sealed partial class BackdropGlassBrush : XamlCompositionBrushBase
{
    private const string BackdropSourceName = "Backdrop";

    public static readonly DependencyProperty BlurAmountProperty = DependencyProperty.Register(
        nameof(BlurAmount),
        typeof(double),
        typeof(BackdropGlassBrush),
        new PropertyMetadata(0d, OnBrushPropertyChanged));

    public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
        nameof(Saturation),
        typeof(double),
        typeof(BackdropGlassBrush),
        new PropertyMetadata(1d, OnBrushPropertyChanged));

    public static readonly DependencyProperty TintColorProperty = DependencyProperty.Register(
        nameof(TintColor),
        typeof(Color),
        typeof(BackdropGlassBrush),
        new PropertyMetadata(Colors.Transparent, OnBrushPropertyChanged));

    public double BlurAmount
    {
        get => (double)GetValue(BlurAmountProperty);
        set => SetValue(BlurAmountProperty, value);
    }

    public double Saturation
    {
        get => (double)GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        Rebuild();
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        Release();
    }

    private static void OnBrushPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is BackdropGlassBrush brush)
        {
            brush.Rebuild();
        }
    }

    private void Rebuild()
    {
        var compositor = CompositionTarget.GetCompositorForCurrentThread();
        if (compositor is null)
        {
            return;
        }

        Release();

        var blur = new GaussianBlurEffect
        {
            BlurAmount = (float)BlurAmount,
            BorderMode = EffectBorderMode.Hard,
            Optimization = EffectOptimization.Balanced,
            Source = new CompositionEffectSourceParameter(BackdropSourceName),
        };

        var saturate = new SaturationEffect
        {
            Saturation = (float)Saturation,
            Source = blur,
        };

        var tint = new ColorSourceEffect { Color = TintColor };
        var composite = new CompositeEffect { Mode = CanvasComposite.SourceOver };
        composite.Sources.Add(saturate);
        composite.Sources.Add(tint);

        using var factory = compositor.CreateEffectFactory(composite);
        var brush = factory.CreateBrush();
        brush.SetSourceParameter(BackdropSourceName, compositor.CreateBackdropBrush());
        CompositionBrush = brush;
    }

    private void Release()
    {
        CompositionBrush?.Dispose();
        CompositionBrush = null;
    }
}
