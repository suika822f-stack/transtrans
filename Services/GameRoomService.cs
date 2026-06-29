using System.Collections.Concurrent;
using TransTrans.Models;

namespace TransTrans.Services;

public class GameRoomService
{
    private readonly ConcurrentDictionary<string, GameRoom> rooms = new();
    private readonly Random random = new();

    public GameRoom CreateRoom()
    {
        string code;
        do
        {
            code = random.Next(1000, 10000).ToString();
        }
        while (rooms.ContainsKey(code));

        var room = new GameRoom
        {
            Code = code,
            State = CreateGame()
        };

        rooms[code] = room;
        return room;
    }

    public GameRoom? GetRoom(string code)
    {
        return rooms.TryGetValue(code.Trim(), out var room) ? room : null;
    }

    public int Join(GameRoom room)
    {
        if (!room.Player1Joined)
        {
            room.Player1Joined = true;
            room.NotifyChanged();
            return 1;
        }

        if (!room.Player2Joined)
        {
            room.Player2Joined = true;
            room.NotifyChanged();
            return 2;
        }

        return 0;
    }

    private GameState CreateGame()
    {
        var game = new GameState();
        game.Player1.ResearchDeck = ResearchCatalog.CreateDeck();
        game.Player2.ResearchDeck = ResearchCatalog.CreateDeck();
        game.PendingChoice = new PendingChoice
        {
            Kind = PendingChoiceKind.RevealResearch,
            PlayerNumber = 1,
            CandidateIds = InitialResearchCandidates(game.Player1),
            Message = "このターンに公開する研究を選んでください。"
        };

        return game;
    }

    private static List<Guid> InitialResearchCandidates(PlayerState player) =>
        player.ResearchDeck
            .Where(x => x.Rank is AlchemyRank.LowerComposite or AlchemyRank.LowerPurification)
            .Select(x => x.Id)
            .ToList();
}
