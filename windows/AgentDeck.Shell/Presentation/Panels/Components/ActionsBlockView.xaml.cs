using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Panels.Components;

public sealed partial class ActionsBlockView : UserControl
{
    private readonly Func<PanelButton, Task> _invoke;

    public ActionsBlockView(ActionsBlock block, Func<PanelButton, Task> invoke)
    {
        InitializeComponent();
        _invoke = invoke;

        foreach (var definition in block.Buttons)
        {
            Buttons.Children.Add(BuildButton(definition));
        }
    }

    private Button BuildButton(PanelButton definition)
    {
        var button = new Button
        {
            Content = definition.Label,
            Background = PanelResources.Brush(PanelMetrics.GradientBrand),
            Foreground = PanelResources.Brush(PanelMetrics.TextOnBrand),
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.UiSize),
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            CornerRadius = (CornerRadius)Application.Current.Resources[PanelMetrics.RadiusSm],
            Padding = (Thickness)Application.Current.Resources[PanelMetrics.PadButton],
        };

        button.Click += async (_, _) => await _invoke(definition);
        return button;
    }
}
