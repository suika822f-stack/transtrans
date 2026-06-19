namespace TransTrans.Models;

public class GameRoom
{
    public string Code { get; init; } = "";
    public GameState State { get; init; } = new();
    public bool Player1Joined { get; set; }
    public bool Player2Joined { get; set; }
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}
