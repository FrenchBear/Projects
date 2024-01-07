// QuirkleUI
// Model
//
// 2023-12-11   PV      First version

using LibQwirkle;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Documents;

namespace QwirkleUI;

internal class Model(/*MainViewModel viewModel*/)
{
    //private readonly MainViewModel viewModel = viewModel;               // Useful?
    public Board Board = new();
    public Bag Bag = new();
    public Player[] Players = [];

    public int PlayerIndex = 0;
    public int PlayersCount = 1;

    public int RoundNumber = 1;

    public Player CurrentPlayer => Players[PlayerIndex];

    internal void NewPlayers()
    {
        Players = new Player[PlayersCount];
        for (int p = 0; p < PlayersCount; p++)
            Players[p] = new Player { Index = p };
    }

    /// <summary>
    /// Initialize model data for a new game, except players information that is preserved
    /// </summary>
    /// <param name="initialBagTiles">If null, then a random bag is generated, otherwise the bag is initialized with provided list of tiles</param>
    internal void NewBoard(List<Tile>? initialBagTiles = null)
    {
        Board = new Board();
        Bag = new Bag(initialBagTiles);
        PlayerIndex = 0;
        RoundNumber = 1;
    }

    internal (bool, string) EvaluateMoves(Moves moves)
        => Board.EvaluateMoves(moves);

    internal PointsBonus CountPoints(Moves moves)
        => Board.CountPoints(moves);

    internal void UpdateRanks()
    {
        // No ranking for single player
        if (PlayersCount == 1)
        {
            Players[0].Rank = "";
            return;
        }

        var l = Players.Select(p => (p.Index, p.Score)).OrderByDescending(it => it.Score).ToList();
        int r = 1;
        int s = l.First().Score;
        int n = 0;
        foreach (var (index, score) in l)
        {
            if (score == s)
            {
                Players[index].Rank = r == 1 ? "1er" : $"{r}è";
                n++;
                continue;
            }
            s = score;
            r += n;
            n = 1;
            Players[index].Rank = $"{r}è";
        }
    }

    public bool IsBagEmpty => Bag.IsEmpty;
}
