namespace TransTrans.Models;

public enum PendingChoiceKind
{
    None,
    SecondPlayerStartingElement,
    RevealResearch,
    ThunderSteal,
    ObsidianSeal,
    ClayCreate,
    MossAge,
    PotteryCreate
}

public class PendingChoice
{
    public PendingChoiceKind Kind { get; set; }
    public int PlayerNumber { get; set; }
    public Guid? SourceItemId { get; set; }
    public string Message { get; set; } = "";
    public List<Guid> CandidateIds { get; set; } = [];
}
