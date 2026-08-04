using AgentDeck.Shell.Presentation.Git.Utils;
using AgentDeck.Shell.Presentation.Panels.Utils;

using Microsoft.UI.Xaml.Data;

namespace AgentDeck.Shell.Presentation.Workspace.Utils;

public sealed class ExplorerStatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is string kind && kind.Length > 0
            ? GitStatusPalette.Brush(kind)
            : PanelResources.Brush(PanelMetrics.TextPrimary);

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
