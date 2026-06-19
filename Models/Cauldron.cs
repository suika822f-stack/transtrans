namespace TransTrans.Models;

public class Cauldron
{
    public List<Card> Cards { get; set; } = [];
    public EnvironmentType Environment { get; set; } = EnvironmentType.None;
}
