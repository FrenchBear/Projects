using System;
using System.Collections.Generic;
using System.Text;

namespace SolWPF
{
    internal partial class GameDataBag
    {
        internal MovingGroup GetNextMove()
        {
            // ToDp: Just for tests, to replace by real solver code
            var hitList = new List<PlayingCard>();
            for (int i = 0; i < 7; i++)
                if (Columns[i].PlayingCards.Count > 0)
                {
                    hitList.Add(Columns[i].PlayingCards[0]);
                    var mg = new MovingGroup(Columns[i], hitList, true, false);
                    mg.ToStack = Bases[Columns[i].PlayingCards[0].Color];
                    return mg;
                }
            return null;
        }


    }
}
