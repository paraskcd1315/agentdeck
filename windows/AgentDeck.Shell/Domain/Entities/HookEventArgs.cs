namespace AgentDeck.Shell.Domain.Entities;

public sealed class HookEventArgs : EventArgs
{
    public HookEventArgs(string payload)
    {
        Payload = payload;
    }

    public string Payload { get; }
}
