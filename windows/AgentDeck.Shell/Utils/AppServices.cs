using AgentDeck.Shell.Data.Daemon;
using AgentDeck.Shell.Data.Localization;
using AgentDeck.Shell.Domain.Interfaces;

namespace AgentDeck.Shell.Utils;

public static class AppServices
{
    private static readonly Lazy<IDaemonClient> DaemonClientInstance = new(() => new DaemonClient());
    private static readonly Lazy<IStringProvider> StringProviderInstance = new(() => new ResourceStringProvider());

    public static IDaemonClient Daemon => DaemonClientInstance.Value;

    public static IStringProvider Strings => StringProviderInstance.Value;
}
