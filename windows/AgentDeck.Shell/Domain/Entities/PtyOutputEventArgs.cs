namespace AgentDeck.Shell.Domain.Entities;

public sealed class PtyOutputEventArgs : EventArgs
{
    public PtyOutputEventArgs(long ptyId, string text)
    {
        PtyId = ptyId;
        Text = text;
    }

    public long PtyId { get; }

    public string Text { get; }
}
