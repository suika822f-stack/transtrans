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
        var game = new GameState
        {
            ResearchDeck = ResearchCatalog.CreateDeck()
        };

        var first = game.ResearchDeck
            .FirstOrDefault(x => x.Rank == AlchemyRank.LowerComposite);

        if (first is not null)
        {
            first.IsRevealed = true;
        }

        return game;
    }
}
