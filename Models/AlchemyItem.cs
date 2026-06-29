namespace TransTrans.Models;

public class AlchemyItem : Card
{
    public AlchemyRank Rank { get; set; }
    public bool HasBeenUsed { get; set; }

    public override bool CanPutIntoCauldron =>
        Rank is AlchemyRank.LowerComposite
            or AlchemyRank.LowerPurification
            or AlchemyRank.MiddlePurification;

    public override bool CanUse => Rank == AlchemyRank.UpperComposite && !HasBeenUsed;
}
