namespace TransTrans.Models;

public class ResearchCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public List<string> Recipe { get; set; } = [];
    public int Cost { get; set; }
    public AlchemyRank Rank { get; set; }
    public string Description { get; set; } = "";
    public bool IsRevealed { get; set; }
    public bool IsUnlocked { get; set; }
    public int SealedUntilTurn { get; set; }

    public bool IsSealed(int turn) => turn <= SealedUntilTurn;

    public string RankText => Rank switch
    {
        AlchemyRank.LowerComposite => "下級複合",
        AlchemyRank.UpperComposite => "上級複合",
        AlchemyRank.LowerPurification => "下級純化",
        AlchemyRank.MiddlePurification => "中級純化",
        AlchemyRank.UpperPurification => "上級純化",
        AlchemyRank.PhilosopherStone => "賢者の石",
        _ => Rank.ToString()
    };

    public string RecipeText => string.Join(" + ", Recipe);
}
