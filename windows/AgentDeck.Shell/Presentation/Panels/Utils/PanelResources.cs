using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class PanelResources
{
    public static FontFamily Font(string key) => Resource<FontFamily>(key);

    public static Brush Brush(string key) => Resource<Brush>(key);

    public static double Size(string key) => Resource<double>(key);

    private static T Resource<T>(string key) => (T)Application.Current.Resources[key];
}
