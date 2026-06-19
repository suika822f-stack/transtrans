namespace TransTrans.Models;

public class PlayerState
{
    public string Name { get; set; } = "";
    public int Elixir { get; set; } = 5;
    public int MaxElixir { get; set; } = 5;
    public List<Card> Hand { get; set; } = [];
    public EnvironmentType? FreeGatherEnvironmentUsed { get; set; }
}
