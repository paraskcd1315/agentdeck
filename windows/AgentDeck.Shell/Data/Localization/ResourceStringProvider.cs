using AgentDeck.Shell.Domain.Interfaces;

using Microsoft.Windows.ApplicationModel.Resources;

namespace AgentDeck.Shell.Data.Localization;

public sealed class ResourceStringProvider : IStringProvider
{
    private readonly ResourceLoader _loader = new();

    public string Get(string key) => _loader.GetString(key);

    public string Format(string key, params object[] arguments) =>
        string.Format(Get(key), arguments);
}
