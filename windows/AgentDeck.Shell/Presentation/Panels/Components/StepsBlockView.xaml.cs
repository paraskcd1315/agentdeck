using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class StepsBlockView : UserControl
{
    private const string PendingState = "pending";

    public StepsBlockView(StepsBlock block)
    {
        InitializeComponent();

        foreach (var step in block.Steps)
        {
            Steps.Children.Add(BuildStep(step));
        }
    }

    private static StackPanel BuildStep(PanelStep step)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PanelMetrics.StepRowSpacing,
        };

        row.Children.Add(new Border
        {
            Width = PanelMetrics.StepDotSize,
            Height = PanelMetrics.StepDotSize,
            CornerRadius = new CornerRadius(PanelMetrics.StepDotSize / 2),
            Background = PanelStepBrush.Resolve(step.State),
            VerticalAlignment = VerticalAlignment.Center,
        });

        row.Children.Add(new TextBlock
        {
            Text = step.Label,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = PanelResources.Font(PanelMetrics.FontUi),
            FontSize = PanelResources.Size(PanelMetrics.BodySize),
            Foreground = PanelResources.Brush(step.State == PendingState
                ? PanelMetrics.TextSecondary
                : PanelMetrics.TextPrimary),
            VerticalAlignment = VerticalAlignment.Center,
        });

        return row;
    }
}
