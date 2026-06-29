namespace TransTrans.Models;

public class GameState
{
    public PlayerState Player1 { get; set; } = new() { Name = "Player 1" };
    public PlayerState Player2 { get; set; } = new() { Name = "Player 2" };
    public Cauldron Cauldron { get; set; } = new();
    public int Turn { get; set; } = 1;
    private string log = "ゲームを開始しました。";
    public string Log
    {
        get => log;
        set
        {
            log = value;
            LogEntries.Insert(0, $"Turn {Turn}: {value}");

            if (LogEntries.Count > 80)
            {
                LogEntries.RemoveRange(80, LogEntries.Count - 80);
            }
        }
    }

    public List<string> LogEntries { get; set; } = ["Turn 1: ゲームを開始しました。"];

    public int CrystalBonusTurn { get; set; }
    public int SwampTurn { get; set; }
    public int ResearchDiscountTurn { get; set; }
    public int SulfurTurn { get; set; }
    public int MistTurn { get; set; }
    public bool SkipAgingThisTurn { get; set; }
    public int? WinnerPlayerNumber { get; set; }
    public string WinReason { get; set; } = "";

    public PendingChoice? PendingChoice { get; set; }

    public PlayerState CurrentPlayer => Turn % 2 == 1 ? Player1 : Player2;
    public PlayerState OpponentPlayer => Turn % 2 == 1 ? Player2 : Player1;
    public int CurrentPlayerNumber => Turn % 2 == 1 ? 1 : 2;

    public PlayerState Player(int number) => number == 1 ? Player1 : Player2;
    public PlayerState Opponent(int number) => number == 1 ? Player2 : Player1;
}
