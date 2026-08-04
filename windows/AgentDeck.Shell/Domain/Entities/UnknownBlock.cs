namespace AgentDeck.Shell.Domain.Entities;

public sealed class UnknownBlock : PanelBlock
{
    public string Type { get; init; } = string.Empty;

    public string Raw { get; init; } = string.Empty;
}
