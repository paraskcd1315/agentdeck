using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace AgentDeck.Shell.Presentation.Panels.Utils;

public static class PanelResources
{
    private const string DarkKey = "Dark";
    private const string LightKey = "Light";

    public static FontFamily Font(string key) => Resolve<FontFamily>(key);

    public static Brush Brush(string key) => Resolve<Brush>(key);

    public static double Size(string key) => Resolve<double>(key);

    private static T Resolve<T>(string key)
    {
        var resources = Application.Current.Resources;

        if (resources.ThemeDictionaries.TryGetValue(ActiveThemeKey(), out var themed)
            && themed is ResourceDictionary dictionary
            && dictionary.TryGetValue(key, out var themedValue))
        {
            return (T)themedValue;
        }

        return (T)resources[key];
    }

    private static string ActiveThemeKey() =>
        Application.Current.RequestedTheme == ApplicationTheme.Light ? LightKey : DarkKey;
}
