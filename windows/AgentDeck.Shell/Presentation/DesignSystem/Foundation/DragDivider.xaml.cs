using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace AgentDeck.Shell.Presentation.DesignSystem.Foundation;

public sealed partial class DragDivider : UserControl
{
    private const string HoverKey = "AdStrokeStrongBrush";
    private const string RestKey = "AdStrokeBrush";
    private const double Thickness = 7;
    private const double RuleThickness = 1;

    private bool _vertical;

    public DragDivider()
    {
        InitializeComponent();

        SetVertical(false);

        ManipulationDelta += OnManipulationDelta;
        PointerEntered += (_, _) => Rule.Background = PanelResources.Brush(HoverKey);
        PointerExited += (_, _) => Rule.Background = PanelResources.Brush(RestKey);
    }

    public event EventHandler<double>? Dragged;

    public void SetVertical(bool vertical)
    {
        _vertical = vertical;

        ManipulationMode = vertical ? ManipulationModes.TranslateY : ManipulationModes.TranslateX;
        ProtectedCursor = InputSystemCursor.Create(vertical
            ? InputSystemCursorShape.SizeNorthSouth
            : InputSystemCursorShape.SizeWestEast);

        if (vertical)
        {
            Width = double.NaN;
            Height = Thickness;
            Rule.Width = double.NaN;
            Rule.Height = RuleThickness;
            Rule.HorizontalAlignment = HorizontalAlignment.Stretch;
            Rule.VerticalAlignment = VerticalAlignment.Center;
            return;
        }

        Width = Thickness;
        Height = double.NaN;
        Rule.Width = RuleThickness;
        Rule.Height = double.NaN;
        Rule.HorizontalAlignment = HorizontalAlignment.Center;
        Rule.VerticalAlignment = VerticalAlignment.Stretch;
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
    {
        args.Handled = true;
        Dragged?.Invoke(this, _vertical ? args.Delta.Translation.Y : args.Delta.Translation.X);
    }
}
