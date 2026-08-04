using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.DesignSystem.TextGrid;

public sealed partial class TextGridScrollBar : UserControl
{
    public TextGridScrollBar()
    {
        InitializeComponent();
        Width = TextGridScrollMetrics.ThumbWidth;
        Thumb.Width = TextGridScrollMetrics.ThumbWidth;
    }

    public void Update(int history, int displayOffset, int rows)
    {
        if (history <= 0 || rows <= 0 || ActualHeight <= 0)
        {
            Visibility = Visibility.Collapsed;
            return;
        }

        Visibility = Visibility.Visible;

        var total = history + rows;
        var height = Math.Max(
            TextGridScrollMetrics.MinimumThumbHeight,
            ActualHeight * rows / total);
        var travel = Math.Max(0, ActualHeight - height);
        var fromBottom = travel * Math.Clamp(displayOffset, 0, history) / history;

        Thumb.Height = height;
        Thumb.Margin = new Thickness(0, travel - fromBottom, 0, 0);
    }
}
