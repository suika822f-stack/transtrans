using TransTrans.Models;

namespace TransTrans.Services;

public static class ResearchCatalog
{
    public static List<ResearchCard> CreateDeck()
    {
        return
        [
            New("蒸気", ["火", "水"], 1, AlchemyRank.LowerComposite),
            New("泥", ["水", "土"], 1, AlchemyRank.LowerComposite),
            New("溶岩", ["火", "土"], 1, AlchemyRank.LowerComposite),
            New("砂", ["風", "土"], 1, AlchemyRank.LowerComposite),
            New("煙", ["火", "風"], 1, AlchemyRank.LowerComposite),
            New("雲", ["水", "風"], 1, AlchemyRank.LowerComposite),
            New("霧", ["蒸気", "煙"], 2, AlchemyRank.UpperComposite, "次の相手ターン中、相手は上級複合錬成物を使用できない"),
            New("黒曜石", ["蒸気", "溶岩"], 2, AlchemyRank.UpperComposite, "研究済みの錬成物を1つ選び、次の相手ターン中錬成不可にする"),
            New("雨", ["蒸気", "雲"], 2, AlchemyRank.UpperComposite, "このターン、ターン終了しても大釜の中の素材が熟成しない"),
            New("温泉", ["蒸気", "泥"], 2, AlchemyRank.UpperComposite, "エリクサー+3"),
            New("ガラス", ["蒸気", "砂"], 2, AlchemyRank.UpperComposite, "次の研究に必要なエリクサーを2減らす"),
            New("硫黄", ["煙", "溶岩"], 2, AlchemyRank.UpperComposite, "次の相手ターン中、採取に必要なエリクサー+1"),
            New("雷", ["煙", "雲"], 2, AlchemyRank.UpperComposite, "相手の手札から1枚奪う"),
            New("沼", ["煙", "泥"], 2, AlchemyRank.UpperComposite, "次の相手ターン開始時、エリクサー-1"),
            New("灰", ["煙", "砂"], 2, AlchemyRank.UpperComposite, "大釜に熟成済みの風と土を加える"),
            New("火山", ["溶岩", "雲"], 2, AlchemyRank.UpperComposite, "手札に溶岩と雲と煙を加える"),
            New("粘土", ["溶岩", "泥"], 2, AlchemyRank.UpperComposite, "任意の下級錬成物を手札に加える"),
            New("結晶", ["溶岩", "砂"], 2, AlchemyRank.UpperComposite, "エリクサー+2。次の自分のターン開始時にもエリクサー+2"),
            New("苔", ["雲", "泥"], 2, AlchemyRank.UpperComposite, "大釜の中の素材を1つ選び熟成させる"),
            New("嵐", ["雲", "砂"], 2, AlchemyRank.UpperComposite, "自分と相手の手札をすべて破壊する"),
            New("陶器", ["泥", "砂"], 2, AlchemyRank.UpperComposite, "上級純化錬成物を1つ手札に加える"),
            New("火精", ["火", "火"], 2, AlchemyRank.LowerPurification),
            New("水精", ["水", "水"], 2, AlchemyRank.LowerPurification),
            New("風精", ["風", "風"], 2, AlchemyRank.LowerPurification),
            New("土精", ["土", "土"], 2, AlchemyRank.LowerPurification),
            New("火核", ["火精", "火精"], 2, AlchemyRank.MiddlePurification),
            New("水核", ["水精", "水精"], 2, AlchemyRank.MiddlePurification),
            New("風核", ["風精", "風精"], 2, AlchemyRank.MiddlePurification),
            New("土核", ["土精", "土精"], 2, AlchemyRank.MiddlePurification),
            New("火王", ["火核", "火核"], 2, AlchemyRank.UpperPurification),
            New("水王", ["水核", "水核"], 2, AlchemyRank.UpperPurification),
            New("風王", ["風核", "風核"], 2, AlchemyRank.UpperPurification),
            New("土王", ["土核", "土核"], 2, AlchemyRank.UpperPurification)
        ];
    }

    private static ResearchCard New(
        string name,
        IEnumerable<string> recipe,
        int cost,
        AlchemyRank rank,
        string description = "")
    {
        return new ResearchCard
        {
            Name = name,
            Recipe = recipe.ToList(),
            Cost = cost,
            Rank = rank,
            Description = description
        };
    }
}
