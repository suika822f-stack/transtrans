using TransTrans.Models;

namespace TransTrans.Services;

public class GameEngine
{
    public void TakeElement(GameRoom room, int playerNumber, ElementType element)
    {
        Mutate(room, playerNumber, game =>
        {
            var cost = GetGatherCost(game, element);
            if (game.CurrentPlayer.Elixir < cost)
            {
                game.Log = "エリクサーが不足しています。";
                return;
            }

            game.CurrentPlayer.Elixir -= cost;
            game.CurrentPlayer.Hand.Add(new ElementCard
            {
                Name = ElementName(element),
                Element = element
            });

            var env = game.Cauldron.Environment;
            if (MatchesEnvironment(env, element) &&
                game.CurrentPlayer.FreeGatherEnvironmentUsed != env)
            {
                game.CurrentPlayer.FreeGatherEnvironmentUsed = env;
            }

            if (game.Turn == game.SulfurTurn)
            {
                game.SulfurTurn = 0;
            }

            game.Log = $"{game.CurrentPlayer.Name} が {ElementName(element)} を採取しました。";
        });
    }

    public void PutIntoCauldron(GameRoom room, int playerNumber, Guid cardId)
    {
        Mutate(room, playerNumber, game =>
        {
            var card = game.CurrentPlayer.Hand.FirstOrDefault(x => x.Id == cardId);
            if (card is null || !card.CanPutIntoCauldron)
            {
                return;
            }

            game.CurrentPlayer.Hand.Remove(card);
            card.IsReady = false;
            game.Cauldron.Cards.Add(card);
            game.Log = $"{card.Name} を大釜に入れました。";
        });
    }

    public void Research(GameRoom room, int playerNumber, Guid researchId)
    {
        Mutate(room, playerNumber, game =>
        {
            var research = game.CurrentPlayer.ResearchDeck.FirstOrDefault(x => x.Id == researchId);
            if (research is null || research.IsUnlocked || !research.IsRevealed || research.IsSealed(game.Turn))
            {
                return;
            }

            var actualCost = GetResearchCost(game, research);
            if (game.CurrentPlayer.Elixir < actualCost)
            {
                game.Log = "エリクサーが不足しています。";
                return;
            }

            game.CurrentPlayer.Elixir -= actualCost;
            research.IsUnlocked = true;
            if (game.Turn == game.ResearchDiscountTurn)
            {
                game.ResearchDiscountTurn = 0;
            }

            game.Log = $"{game.CurrentPlayer.Name} が研究を1件完了しました。";
        });
    }

    public void Craft(GameRoom room, int playerNumber, Guid researchId)
    {
        Mutate(room, playerNumber, game =>
        {
            var research = game.CurrentPlayer.ResearchDeck.FirstOrDefault(x => x.Id == researchId);
            if (research is null || !CanCraft(game, research, playerNumber))
            {
                return;
            }

            game.CurrentPlayer.Elixir--;

            var materials = game.Cauldron.Cards.ToList();
            foreach (var recipeName in research.Recipe)
            {
                var material = materials.First(x => x.IsReady && x.Name == recipeName);
                game.Cauldron.Cards.Remove(material);
                materials.Remove(material);
            }

            game.CurrentPlayer.Hand.Add(new AlchemyItem
            {
                Name = research.Name,
                Rank = research.Rank
            });

            game.Log = $"{research.Name} を錬成しました。";
            CheckWin(game, game.CurrentPlayerNumber);
        });
    }

    public void UseItem(GameRoom room, int playerNumber, Guid itemId)
    {
        Mutate(room, playerNumber, game =>
        {
            var item = game.CurrentPlayer.Hand.OfType<AlchemyItem>().FirstOrDefault(x => x.Id == itemId);
            if (item is null || !item.CanUse || item.HasBeenUsed)
            {
                return;
            }

            if (item.Rank == AlchemyRank.UpperComposite && game.Turn == game.MistTurn)
            {
                game.Log = "霧の効果で上級複合錬成物は使用できません。";
                return;
            }

            var completed = ExecuteItem(game, playerNumber, item);
            if (completed)
            {
                item.HasBeenUsed = true;
                CheckWin(game, playerNumber);
            }
        });
    }

    public void PlaceStartingElement(GameRoom room, int playerNumber, ElementType element)
    {
        Mutate(room, playerNumber, game =>
        {
            if (game.Turn != 2 ||
                playerNumber != 2 ||
                game.CurrentPlayer.HasPlacedStartingElement ||
                game.PendingChoice?.Kind != PendingChoiceKind.SecondPlayerStartingElement)
            {
                return;
            }

            game.Cauldron.Cards.Add(new ElementCard
            {
                Name = ElementName(element),
                Element = element,
                IsReady = true
            });
            game.CurrentPlayer.HasPlacedStartingElement = true;
            game.PendingChoice = null;
            game.Log = "後攻プレイヤーが熟成済みの元素を大釜に加えました。";
            QueueResearchChoice(game);
        }, allowPending: true);
    }

    public void ResolveChoice(GameRoom room, int playerNumber, Guid selectedId)
    {
        Mutate(room, playerNumber, game =>
        {
            var pending = game.PendingChoice;
            if (pending is null ||
                pending.PlayerNumber != playerNumber ||
                !pending.CandidateIds.Contains(selectedId))
            {
                return;
            }

            var player = game.Player(playerNumber);
            var opponent = game.Opponent(playerNumber);
            var item = pending.SourceItemId is null
                ? null
                : player.Hand.OfType<AlchemyItem>().FirstOrDefault(x => x.Id == pending.SourceItemId);

            switch (pending.Kind)
            {
                case PendingChoiceKind.RevealResearch:
                    var reveal = player.ResearchDeck.First(x => x.Id == selectedId);
                    reveal.IsRevealed = true;
                    game.Log = $"{player.Name} が研究カードを1枚選びました。";
                    break;

                case PendingChoiceKind.ThunderSteal:
                    var stolen = opponent.Hand.First(x => x.Id == selectedId);
                    opponent.Hand.Remove(stolen);
                    player.Hand.Add(stolen);
                    CompleteItemUse(player, item);
                    game.Log = $"{stolen.Name} を奪いました。";
                    CheckWin(game, playerNumber);
                    break;

                case PendingChoiceKind.ObsidianSeal:
                    var sealedResearch = opponent.ResearchDeck.First(x => x.Id == selectedId);
                    sealedResearch.SealedUntilTurn = game.Turn + 1;
                    CompleteItemUse(player, item);
                    game.Log = $"{sealedResearch.Name} を封印しました。";
                    break;

                case PendingChoiceKind.ClayCreate:
                case PendingChoiceKind.PotteryCreate:
                    var created = player.ResearchDeck.First(x => x.Id == selectedId);
                    player.Hand.Add(new AlchemyItem
                    {
                        Name = created.Name,
                        Rank = created.Rank
                    });
                    CompleteItemUse(player, item);
                    game.Log = $"{created.Name} を手札に加えました。";
                    CheckWin(game, playerNumber);
                    break;

                case PendingChoiceKind.MossAge:
                    var aged = game.Cauldron.Cards.First(x => x.Id == selectedId);
                    aged.IsReady = true;
                    CompleteItemUse(player, item);
                    game.Log = $"{aged.Name} を熟成させました。";
                    break;
            }

            game.PendingChoice = null;
        }, allowPending: true);
    }

    public void CancelChoice(GameRoom room, int playerNumber)
    {
        Mutate(room, playerNumber, game =>
        {
            if (game.PendingChoice?.PlayerNumber == playerNumber &&
                game.PendingChoice.Kind is not PendingChoiceKind.RevealResearch
                    and not PendingChoiceKind.SecondPlayerStartingElement)
            {
                game.PendingChoice = null;
                game.Log = "選択をキャンセルしました。";
            }
        }, allowPending: true);
    }

    public void EndTurn(GameRoom room, int playerNumber)
    {
        Mutate(room, playerNumber, game =>
        {
            if (!game.SkipAgingThisTurn)
            {
                foreach (var card in game.Cauldron.Cards)
                {
                    card.IsReady = true;
                }
            }

            game.SkipAgingThisTurn = false;
            game.CurrentPlayer.Elixir = game.CurrentPlayer.MaxElixir;

            if (game.Turn == game.SulfurTurn)
            {
                game.SulfurTurn = 0;
            }

            game.Turn++;
            game.CurrentPlayer.FreeGatherEnvironmentUsed = null;
            StartTurn(game);
            game.Log = $"{game.CurrentPlayer.Name} のターンです。";
        });
    }

    public bool CanCraft(GameState game, ResearchCard research, int playerNumber)
    {
        var player = game.Player(playerNumber);
        if (!research.IsUnlocked || research.IsSealed(game.Turn) || player.Elixir < 1)
        {
            return false;
        }

        var materials = game.Cauldron.Cards.ToList();
        foreach (var recipeName in research.Recipe)
        {
            var material = materials.FirstOrDefault(x => x.IsReady && x.Name == recipeName);
            if (material is null)
            {
                return false;
            }

            materials.Remove(material);
        }

        return true;
    }

    public IEnumerable<ResearchCard> AvailableResearches(GameState game, int playerNumber) =>
        game.Player(playerNumber).ResearchDeck.Where(x => x.IsRevealed && !x.IsUnlocked && !x.IsSealed(game.Turn));

    public IEnumerable<ResearchCard> CraftableResearches(GameState game, int playerNumber) =>
        game.Player(playerNumber).ResearchDeck.Where(x => CanCraft(game, x, playerNumber));

    public IEnumerable<ResearchCard> RevealCandidates(GameState game) =>
        game.CurrentPlayer.ResearchDeck.Where(x => !x.IsRevealed && CanRevealResearch(game, x));

    public int GetResearchCost(GameState game, ResearchCard research)
    {
        var discount = game.Turn == game.ResearchDiscountTurn ? 2 : 0;
        return Math.Max(0, research.Cost - discount);
    }

    public int GetGatherCost(GameState game, ElementType element)
    {
        var cost = game.Turn == game.SulfurTurn ? 2 : 1;
        var env = game.Cauldron.Environment;
        if (MatchesEnvironment(env, element) &&
            game.CurrentPlayer.FreeGatherEnvironmentUsed != env)
        {
            cost--;
        }

        return Math.Max(0, cost);
    }

    private static void CompleteItemUse(PlayerState player, AlchemyItem? item)
    {
        if (item is not null)
        {
            item.HasBeenUsed = true;
        }
    }

    private bool ExecuteItem(GameState game, int playerNumber, AlchemyItem item)
    {
        switch (item.Name)
        {
            case "霧":
                game.MistTurn = game.Turn + 1;
                game.Log = "霧が立ち込めました。";
                return true;

            case "黒曜石":
                return SetResearchChoice(game, playerNumber, item, PendingChoiceKind.ObsidianSeal,
                    game.Opponent(playerNumber).ResearchDeck.Where(x => x.IsUnlocked).Select(x => x.Id),
                    "封印する研究済みカードを選んでください。");

            case "雨":
                game.SkipAgingThisTurn = true;
                game.Log = "雨により、このターン終了時の熟成が止まります。";
                return true;

            case "温泉":
                game.CurrentPlayer.Elixir += 3;
                game.Log = "温泉でエリクサーを3得ました。";
                return true;

            case "ガラス":
                game.ResearchDiscountTurn = game.Turn + 2;
                game.Log = "次の研究コストが2下がります。";
                return true;

            case "硫黄":
                game.SulfurTurn = game.Turn + 1;
                game.Log = "次の相手ターンの採取コストが上がります。";
                return true;

            case "雷":
                return SetCardChoice(game, playerNumber, item, PendingChoiceKind.ThunderSteal,
                    game.Opponent(playerNumber).Hand.Select(x => x.Id),
                    "奪う相手の手札を選んでください。");

            case "沼":
                game.SwampTurn = game.Turn + 1;
                game.Log = "次の相手ターン開始時、相手のエリクサーが1減ります。";
                return true;

            case "灰":
                game.Cauldron.Cards.Add(new ElementCard { Name = "風", Element = ElementType.Air, IsReady = true });
                game.Cauldron.Cards.Add(new ElementCard { Name = "土", Element = ElementType.Earth, IsReady = true });
                game.Log = "大釜に熟成済みの風と土を加えました。";
                return true;

            case "火山":
                AddItem(game.CurrentPlayer, "溶岩", AlchemyRank.LowerComposite);
                AddItem(game.CurrentPlayer, "雲", AlchemyRank.LowerComposite);
                AddItem(game.CurrentPlayer, "煙", AlchemyRank.LowerComposite);
                game.Log = "溶岩、雲、煙を手札に加えました。";
                return true;

            case "粘土":
                return SetResearchChoice(game, playerNumber, item, PendingChoiceKind.ClayCreate,
                    game.CurrentPlayer.ResearchDeck
                        .Where(x => x.Rank is AlchemyRank.LowerComposite or AlchemyRank.LowerPurification)
                        .Select(x => x.Id),
                    "手札に加える下級錬成物を選んでください。");

            case "結晶":
                game.CurrentPlayer.Elixir += 2;
                game.CrystalBonusTurn = game.Turn + 2;
                game.Log = "エリクサーを2得ました。次の自分のターンにも2得ます。";
                return true;

            case "苔":
                return SetCardChoice(game, playerNumber, item, PendingChoiceKind.MossAge,
                    game.Cauldron.Cards.Where(x => !x.IsReady).Select(x => x.Id),
                    "熟成させる大釜の素材を選んでください。");

            case "嵐":
                game.CurrentPlayer.Hand.Clear();
                game.OpponentPlayer.Hand.Clear();
                game.Log = "嵐がすべての手札を破壊しました。";
                return true;

            case "陶器":
                return SetResearchChoice(game, playerNumber, item, PendingChoiceKind.PotteryCreate,
                    game.CurrentPlayer.ResearchDeck.Where(x => x.Rank == AlchemyRank.UpperPurification).Select(x => x.Id),
                    "手札に加える上級純化錬成物を選んでください。");

            default:
                game.Log = $"{item.Name} の効果はまだありません。";
                return false;
        }
    }

    private static void AddItem(PlayerState player, string name, AlchemyRank rank)
    {
        player.Hand.Add(new AlchemyItem { Name = name, Rank = rank });
    }

    private bool SetCardChoice(
        GameState game,
        int playerNumber,
        AlchemyItem item,
        PendingChoiceKind kind,
        IEnumerable<Guid> candidates,
        string message)
    {
        var ids = candidates.ToList();
        if (ids.Count == 0)
        {
            game.Log = "対象がありません。";
            return false;
        }

        game.PendingChoice = new PendingChoice
        {
            Kind = kind,
            PlayerNumber = playerNumber,
            SourceItemId = item.Id,
            CandidateIds = ids,
            Message = message
        };

        game.Log = message;
        return false;
    }

    private bool SetResearchChoice(
        GameState game,
        int playerNumber,
        AlchemyItem item,
        PendingChoiceKind kind,
        IEnumerable<Guid> candidates,
        string message)
    {
        var ids = candidates.ToList();
        if (ids.Count == 0)
        {
            game.Log = "対象がありません。";
            return false;
        }

        game.PendingChoice = new PendingChoice
        {
            Kind = kind,
            PlayerNumber = playerNumber,
            SourceItemId = item.Id,
            CandidateIds = ids,
            Message = message
        };

        game.Log = message;
        return false;
    }

    private void StartTurn(GameState game)
    {
        if (game.Turn == game.CrystalBonusTurn)
        {
            game.CurrentPlayer.Elixir += 2;
            game.CrystalBonusTurn = 0;
        }

        if (game.Turn == game.SwampTurn)
        {
            game.CurrentPlayer.Elixir = Math.Max(0, game.CurrentPlayer.Elixir - 1);
            game.SwampTurn = 0;
        }

        if (game.Turn == 2 && !game.Player2.HasPlacedStartingElement)
        {
            game.PendingChoice = new PendingChoice
            {
                Kind = PendingChoiceKind.SecondPlayerStartingElement,
                PlayerNumber = 2,
                Message = "後攻1ターン目: 大釜に加える熟成済み元素を選んでください。"
            };
            return;
        }

        QueueResearchChoice(game);
    }

    private void QueueResearchChoice(GameState game)
    {
        var revealCandidates = RevealCandidates(game).Select(x => x.Id).ToList();
        if (revealCandidates.Count > 0)
        {
            game.PendingChoice = new PendingChoice
            {
                Kind = PendingChoiceKind.RevealResearch,
                PlayerNumber = game.CurrentPlayerNumber,
                CandidateIds = revealCandidates,
                Message = "公開する研究を選んでください。"
            };
        }
    }

    private void Mutate(
        GameRoom room,
        int playerNumber,
        Action<GameState> action,
        bool allowPending = false)
    {
        lock (room)
        {
            var game = room.State;
            if (game.WinnerPlayerNumber is not null)
            {
                return;
            }

            if (playerNumber != game.CurrentPlayerNumber)
            {
                return;
            }

            if (!allowPending && game.PendingChoice is not null)
            {
                return;
            }

            action(game);
            room.NotifyChanged();
        }
    }

    private bool CanRevealResearch(GameState game, ResearchCard research)
    {
        var deck = game.CurrentPlayer.ResearchDeck;
        var lowerComposite = deck.Count(x => x.IsRevealed && x.Rank == AlchemyRank.LowerComposite);
        var lowerPurification = deck.Count(x => x.IsRevealed && x.Rank == AlchemyRank.LowerPurification);
        var middlePurification = deck.Count(x => x.IsRevealed && x.Rank == AlchemyRank.MiddlePurification);
        var upperComposite = deck.Count(x => x.IsRevealed && x.Rank == AlchemyRank.UpperComposite);

        return research.Rank switch
        {
            AlchemyRank.LowerComposite => true,
            AlchemyRank.LowerPurification => true,
            AlchemyRank.MiddlePurification => lowerPurification >= 2,
            AlchemyRank.UpperComposite => lowerComposite >= 3,
            AlchemyRank.UpperPurification => middlePurification >= 3,
            AlchemyRank.PhilosopherStone => upperComposite >= 4,
            _ => false
        };
    }

    private static string ElementName(ElementType element) => element switch
    {
        ElementType.Fire => "火",
        ElementType.Water => "水",
        ElementType.Air => "風",
        ElementType.Earth => "土",
        _ => ""
    };

    private static bool MatchesEnvironment(EnvironmentType env, ElementType element) =>
        (env == EnvironmentType.Fire && element == ElementType.Fire)
        || (env == EnvironmentType.Water && element == ElementType.Water)
        || (env == EnvironmentType.Air && element == ElementType.Air)
        || (env == EnvironmentType.Earth && element == ElementType.Earth);

    private void CheckWin(GameState game, int playerNumber)
    {
        if (game.WinnerPlayerNumber is not null)
        {
            return;
        }

        var player = game.Player(playerNumber);
        var items = player.Hand.OfType<AlchemyItem>().ToList();
        var upperCompositeCount = items.Count(x => x.Rank == AlchemyRank.UpperComposite);
        var purificationScore = items.Sum(x => x.Rank switch
        {
            AlchemyRank.LowerPurification => 1,
            AlchemyRank.MiddlePurification => 2,
            AlchemyRank.UpperPurification => 3,
            _ => 0
        });

        if (upperCompositeCount >= 4)
        {
            game.WinnerPlayerNumber = playerNumber;
            game.WinReason = "上級複合錬成物を4つ手札に集めました。";
            game.Log = $"Player {playerNumber} の勝利: {game.WinReason}";
        }
        else if (purificationScore >= 15)
        {
            game.WinnerPlayerNumber = playerNumber;
            game.WinReason = $"純化錬成物を{purificationScore}点分手札に集めました。";
            game.Log = $"Player {playerNumber} の勝利: {game.WinReason}";
        }
    }
}
