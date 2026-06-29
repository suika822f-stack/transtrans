namespace TransTrans.Models;

public class PlayerState
{
    public string Name { get; set; } = "";
    public int Elixir { get; set; } = 3;
    public int MaxElixir { get; set; } = 3;
    public List<Card> Hand { get; set; } = [];
    public List<ResearchCard> ResearchDeck { get; set; } = [];
    public EnvironmentType? FreeGatherEnvironmentUsed { get; set; }
    public bool HasPlacedStartingElement { get; set; }
}
