using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace AgentDeck.Shell.Presentation.DesignSystem.Foundation;

public sealed partial class DragDivider : UserControl
{
    private const string HoverKey = "AdStrokeStrongBrush";
    private const string RestKey = "AdStrokeBrush";

    public DragDivider()
    {
        InitializeComponent();

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);

        ManipulationDelta += OnManipulationDelta;
        PointerEntered += (_, _) => Rule.Background = PanelResources.Brush(HoverKey);
        PointerExited += (_, _) => Rule.Background = PanelResources.Brush(RestKey);
    }

    public event EventHandler<double>? Dragged;

    private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
    {
        args.Handled = true;
        Dragged?.Invoke(this, args.Delta.Translation.X);
    }
}
