namespace AgentDeck.Shell.Domain.Interfaces;

public interface IStringProvider
{
    string Get(string key);

    string Format(string key, params object[] arguments);
}
